# ============================================================
# InsightIQ — Advanced Query Engine
# query_engine.py
# Handles: multi-dim composite labels, multi-measure grouped,
#          dual-axis detection, scale variance analysis
# ============================================================

from typing import List, Dict, Any, Optional
import psycopg2
import psycopg2.extras

# Chart.js palette
COLORS = [
    "#3b82f6", "#10b981", "#f59e0b", "#8b5cf6",
    "#06b6d4", "#ec4899", "#34d399", "#fbbf24",
    "#f87171", "#a78bfa",
]

def build_and_run(
    conn,
    table: str,
    allowed_columns: Dict[str, str],
    dimensions: List[str],
    measures: List[str],
    filters: List[dict],
    limit: int,
    order_by: Optional[str],
    order_dir: str,
) -> Dict[str, Any]:
    """
    Build and execute a dynamic query.
    Returns raw rows + chart-ready payload with multi-dim/multi-msr support.
    """

    # ── 1. Build SELECT ──────────────────────────────────────
    select_parts = []
    group_cols   = []

    for dim in dimensions:
        col = allowed_columns[dim]
        select_parts.append(f"CAST({col} AS TEXT) AS {dim}")
        group_cols.append(col)

    for msr in measures:
        col = allowed_columns[msr]
        if _is_aggregate(col):
            select_parts.append(f"{col} AS {msr}")
        else:
            select_parts.append(f"SUM({col}) AS {msr}")

    sql = f"SELECT {', '.join(select_parts)} FROM {table}"

    # ── 2. WHERE ─────────────────────────────────────────────
    params: List[Any] = []
    where_parts = []
    for f in (filters or []):
        field = f.get("field", "")
        op    = f.get("op", "=")
        value = f.get("value")
        if field in allowed_columns and op in ("=","!=",">","<",">=","<=","IN","LIKE"):
            col = allowed_columns[field]
            if op == "IN" and isinstance(value, list):
                placeholders = ",".join(["%s"] * len(value))
                where_parts.append(f"{col} IN ({placeholders})")
                params.extend(value)
            else:
                where_parts.append(f"{col} {op} %s")
                params.append(value)
    if where_parts:
        sql += " WHERE " + " AND ".join(where_parts)

    # ── 3. GROUP BY ───────────────────────────────────────────
    if group_cols and measures:
        sql += f" GROUP BY {', '.join(group_cols)}"

    # ── 4. ORDER BY ───────────────────────────────────────────
    direction = "DESC" if order_dir == "DESC" else "ASC"
    if order_by and order_by in allowed_columns:
        ob_col = allowed_columns[order_by]
        if _is_aggregate(ob_col):
            sql += f" ORDER BY {ob_col} {direction}"
        else:
            sql += f" ORDER BY SUM({ob_col}) {direction}"
    elif measures:
        first_msr_col = allowed_columns[measures[0]]
        if not _is_aggregate(first_msr_col):
            sql += f" ORDER BY SUM({first_msr_col}) {direction}"

    # ── 5. LIMIT ─────────────────────────────────────────────
    limit = min(limit or 50, 500)
    sql += f" LIMIT {limit}"

    # ── 6. Execute ───────────────────────────────────────────
    cur = conn.cursor(cursor_factory=psycopg2.extras.RealDictCursor)
    try:
        cur.execute(sql, params)
        raw_rows = [dict(r) for r in cur.fetchall()]
    finally:
        cur.close()

    # Sanitize numeric types
    rows = _sanitize_rows(raw_rows)

    # ── 7. Build chart payload ───────────────────────────────
    chart_payload = _build_chart_payload(rows, dimensions, measures)

    return {
        "columns":       list(rows[0].keys()) if rows else dimensions + measures,
        "rows":          rows,
        "count":         len(rows),
        "chart":         chart_payload,
        "sql_debug":     sql,
    }


# ── CHART PAYLOAD BUILDER ────────────────────────────────────

def _build_chart_payload(
    rows: List[Dict],
    dimensions: List[str],
    measures: List[str],
) -> Dict[str, Any]:
    """
    Builds a Chart.js-ready payload handling:
      - Multi-dimension composite labels
      - Multi-measure grouped datasets
      - Dual Y-axis detection (scale variance)
    """
    if not rows:
        return {"labels": [], "datasets": [], "dualAxis": False, "warning": None}

    # ── Composite X labels (multiple dimensions) ──────────────
    def make_label(row: Dict) -> str:
        if not dimensions:
            return "Value"
        parts = [str(row.get(d, "")) for d in dimensions]
        return " / ".join(p for p in parts if p)

    labels = [make_label(r) for r in rows]

    # ── Detect if dual-axis is needed ────────────────────────
    dual_axis = False
    warning   = None
    if len(measures) >= 2:
        scale_groups = _detect_scale_groups(rows, measures)
        dual_axis    = len(scale_groups) > 1

    # ── Build datasets ────────────────────────────────────────
    datasets = []
    for i, msr in enumerate(measures):
        data   = [_to_float(r.get(msr)) for r in rows]
        color  = COLORS[i % len(COLORS)]
        ds: Dict[str, Any] = {
            "label":           msr,
            "data":            data,
            "backgroundColor": color + "cc",
            "borderColor":     color,
            "borderWidth":     2,
            "borderRadius":    5,
        }
        if dual_axis and len(measures) >= 2:
            ds["yAxisID"] = "y" if i == 0 else "y1"
        datasets.append(ds)

    # ── Warning for complex combos ────────────────────────────
    if len(dimensions) > 2 and len(measures) > 2:
        warning = "Complex combination — consider splitting into separate widgets for clarity."

    return {
        "labels":    labels,
        "datasets":  datasets,
        "dualAxis":  dual_axis,
        "warning":   warning,
        "measures":  measures,
        "dimensions": dimensions,
    }


def _detect_scale_groups(rows: List[Dict], measures: List[str]) -> List[List[str]]:
    """
    Groups measures by order-of-magnitude.
    e.g. [revenue=48M, visits=8.4] → two separate groups → dual axis
    """
    if not rows or len(measures) < 2:
        return [measures]

    import math

    def avg_magnitude(msr: str) -> float:
        vals = [abs(_to_float(r.get(msr))) for r in rows if r.get(msr) is not None]
        if not vals:
            return 0
        avg = sum(vals) / len(vals)
        return math.log10(avg + 1)

    magnitudes = {m: avg_magnitude(m) for m in measures}
    base_mag   = magnitudes[measures[0]]

    group_a, group_b = [], []
    for m in measures:
        diff = abs(magnitudes[m] - base_mag)
        if diff > 1.5:          # more than ~30x difference → separate axis
            group_b.append(m)
        else:
            group_a.append(m)

    if not group_b:
        return [group_a]
    return [group_a, group_b]


# ── HELPERS ──────────────────────────────────────────────────

def _is_aggregate(col: str) -> bool:
    return any(a in col.upper() for a in ("COUNT(", "SUM(", "AVG(", "MAX(", "MIN("))

def _to_float(v: Any) -> float:
    try:
        return float(v)
    except (TypeError, ValueError):
        return 0.0

def _sanitize_rows(rows: List[Dict]) -> List[Dict]:
    clean = []
    for row in rows:
        r = {}
        for k, v in row.items():
            if hasattr(v, '__float__') and not isinstance(v, (bool, int)):
                r[k] = float(v)
            elif v is None:
                r[k] = None
            else:
                r[k] = v
        clean.append(r)
    return clean