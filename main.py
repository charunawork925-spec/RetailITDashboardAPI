# ============================================================
# InsightIQ — FastAPI Backend (upgraded)
# main.py  |  uvicorn main:app --reload --port 8000
# ============================================================

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List, Optional
import database as db
import query_engine as qe

app = FastAPI(title="InsightIQ API", version="2.0.0")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:4200"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ── REQUEST MODEL ────────────────────────────────────────────
class QueryRequest(BaseModel):
    dataset:   str
    dimensions: List[str] = []
    measures:   List[str] = []
    filters:    Optional[List[dict]] = []
    limit:      Optional[int] = 50
    order_by:   Optional[str] = None
    order_dir:  Optional[str] = "DESC"

# ── DATASET METADATA ─────────────────────────────────────────
DATASETS_META = {
    "customers": {
        "id": "customers", "label": "Customers", "icon": "👥", "table": "customers",
        "fields": [
            {"key": "gender",             "label": "Gender",               "type": "dim",  "column": "gender"},
            {"key": "age_group",          "label": "Age Group",            "type": "dim",  "column": "age_group"},
            {"key": "religion",           "label": "Religion",             "type": "dim",  "column": "religion"},
            {"key": "area",               "label": "Area / Branch",        "type": "dim",  "column": "area"},
            {"key": "loyalty_tier",       "label": "Loyalty Tier",         "type": "dim",  "column": "loyalty_tier"},
            {"key": "preferred_category", "label": "Preferred Category",   "type": "dim",  "column": "preferred_category"},
            {"key": "avg_cart_size_lkr",  "label": "Avg Cart Size (LKR)",  "type": "msr",  "column": "avg_cart_size_lkr"},
            {"key": "visits_per_month",   "label": "Visits / Month",       "type": "msr",  "column": "visits_per_month"},
            {"key": "total_spend_ytd",    "label": "Total Spend YTD (LKR)","type": "msr",  "column": "total_spend_ytd"},
            {"key": "join_date",          "label": "Join Date",            "type": "date", "column": "join_date"},
        ]
    },
    "products": {
        "id": "products", "label": "Products", "icon": "📦", "table": "products",
        "fields": [
            {"key": "product_name",   "label": "Product Name",      "type": "dim",  "column": "product_name"},
            {"key": "category",       "label": "Category",          "type": "dim",  "column": "category"},
            {"key": "sub_category",   "label": "Sub-Category",      "type": "dim",  "column": "sub_category"},
            {"key": "brand",          "label": "Brand",             "type": "dim",  "column": "brand"},
            {"key": "is_halal",       "label": "Halal Certified",   "type": "bool", "column": "is_halal"},
            {"key": "is_organic",     "label": "Organic",           "type": "bool", "column": "is_organic"},
            {"key": "is_local",       "label": "Locally Sourced",   "type": "bool", "column": "is_local"},
            {"key": "avg_price_lkr",  "label": "Avg Price (LKR)",   "type": "msr",  "column": "avg_price_lkr"},
            {"key": "units_sold_ytd", "label": "Units Sold",        "type": "msr",  "column": "units_sold_ytd"},
            {"key": "revenue_ytd",    "label": "Revenue (LKR)",     "type": "msr",  "column": "revenue_ytd"},
            {"key": "stock_level",    "label": "Stock Level",       "type": "msr",  "column": "stock_level"},
            {"key": "reorder_point",  "label": "Reorder Point",     "type": "msr",  "column": "reorder_point"},
        ]
    },
    "transactions": {
        "id": "transactions", "label": "Transactions", "icon": "🧾", "table": "transactions",
        "fields": [
            {"key": "txn_month",       "label": "Month",             "type": "date", "column": "txn_month"},
            {"key": "day_of_week",     "label": "Day of Week",       "type": "dim",  "column": "TRIM(day_of_week)"},
            {"key": "time_of_day",     "label": "Time of Day",       "type": "dim",  "column": "time_of_day"},
            {"key": "payment_method",  "label": "Payment Method",    "type": "dim",  "column": "payment_method"},
            {"key": "total_value_lkr", "label": "Total Value (LKR)", "type": "msr",  "column": "total_value_lkr"},
            {"key": "discount_lkr",    "label": "Discount (LKR)",    "type": "msr",  "column": "discount_lkr"},
            {"key": "num_items",       "label": "No. of Items",      "type": "msr",  "column": "num_items"},
            {"key": "basket_count",    "label": "Basket Count",      "type": "msr",  "column": "COUNT(txn_id)"},
        ]
    },
    "departments": {
        "id": "departments", "label": "Departments", "icon": "🏬", "table": "departments",
        "fields": [
            {"key": "dept_name",      "label": "Department",           "type": "dim", "column": "dept_name"},
            {"key": "floor_zone",     "label": "Floor / Zone",         "type": "dim", "column": "floor_zone"},
            {"key": "manager",        "label": "Manager",              "type": "dim", "column": "manager"},
            {"key": "staff_count",    "label": "Staff Count",          "type": "msr", "column": "staff_count"},
            {"key": "revenue_target", "label": "Revenue Target (LKR)", "type": "msr", "column": "revenue_target"},
            {"key": "actual_revenue", "label": "Actual Revenue (LKR)", "type": "msr", "column": "actual_revenue"},
            {"key": "shrinkage_pct",  "label": "Shrinkage %",          "type": "msr", "column": "shrinkage_pct"},
        ]
    },
    "branches": {
        "id": "branches", "label": "Branches / Locations", "icon": "📍", "table": "branches",
        "fields": [
            {"key": "branch_name",      "label": "Branch Name",    "type": "dim",  "column": "branch_name"},
            {"key": "city",             "label": "City",           "type": "dim",  "column": "city"},
            {"key": "region",           "label": "Region",         "type": "dim",  "column": "region"},
            {"key": "manager",          "label": "Manager",        "type": "dim",  "column": "manager"},
            {"key": "branch_size_sqft", "label": "Size (sqft)",    "type": "msr",  "column": "branch_size_sqft"},
            {"key": "daily_footfall",   "label": "Daily Footfall", "type": "msr",  "column": "daily_footfall"},
            {"key": "opened_year",      "label": "Year Opened",    "type": "date", "column": "opened_year"},
        ]
    },
    "suppliers": {
        "id": "suppliers", "label": "Suppliers", "icon": "🚛", "table": "suppliers",
        "fields": [
            {"key": "supplier_name",       "label": "Supplier Name",      "type": "dim",  "column": "supplier_name"},
            {"key": "origin_country",      "label": "Origin Country",     "type": "dim",  "column": "origin_country"},
            {"key": "halal_certified",     "label": "Halal Certified",    "type": "bool", "column": "halal_certified"},
            {"key": "organic_certified",   "label": "Organic Certified",  "type": "bool", "column": "organic_certified"},
            {"key": "lead_time_days",      "label": "Lead Time (days)",   "type": "msr",  "column": "lead_time_days"},
            {"key": "on_time_delivery_pct","label": "On-Time Delivery %", "type": "msr",  "column": "on_time_delivery_pct"},
            {"key": "total_orders_ytd",    "label": "Total Orders YTD",   "type": "msr",  "column": "total_orders_ytd"},
            {"key": "spend_ytd",           "label": "Spend YTD (LKR)",    "type": "msr",  "column": "spend_ytd"},
        ]
    },
    "promotions": {
        "id": "promotions", "label": "Promotions", "icon": "🎯", "table": "promotions",
        "fields": [
            {"key": "promo_name",        "label": "Promotion Name",       "type": "dim",  "column": "promo_name"},
            {"key": "promo_type",        "label": "Promo Type",           "type": "dim",  "column": "promo_type"},
            {"key": "target_segment",    "label": "Target Segment",       "type": "dim",  "column": "target_segment"},
            {"key": "start_date",        "label": "Start Date",           "type": "date", "column": "start_date"},
            {"key": "end_date",          "label": "End Date",             "type": "date", "column": "end_date"},
            {"key": "discount_pct",      "label": "Discount %",           "type": "msr",  "column": "discount_pct"},
            {"key": "redemptions",       "label": "Redemptions",          "type": "msr",  "column": "redemptions"},
            {"key": "revenue_uplift_lkr","label": "Revenue Uplift (LKR)", "type": "msr",  "column": "revenue_uplift_lkr"},
            {"key": "roi_pct",           "label": "ROI %",                "type": "msr",  "column": "roi_pct"},
        ]
    },
    "inventory": {
        "id": "inventory", "label": "Inventory", "icon": "🏪", "table": "inventory",
        "fields": [
            {"key": "sku",               "label": "SKU",                "type": "dim",  "column": "sku"},
            {"key": "warehouse_zone",    "label": "Warehouse Zone",     "type": "dim",  "column": "warehouse_zone"},
            {"key": "quantity_on_hand",  "label": "Qty On Hand",        "type": "msr",  "column": "quantity_on_hand"},
            {"key": "quantity_on_order", "label": "Qty On Order",       "type": "msr",  "column": "quantity_on_order"},
            {"key": "days_of_supply",    "label": "Days of Supply",     "type": "msr",  "column": "days_of_supply"},
            {"key": "shelf_life_days",   "label": "Shelf Life (days)",  "type": "msr",  "column": "shelf_life_days"},
            {"key": "expiry_risk_units", "label": "Expiry Risk Units",  "type": "msr",  "column": "expiry_risk_units"},
            {"key": "last_received_date","label": "Last Received",      "type": "date", "column": "last_received_date"},
        ]
    },
}

def _get_allowed(dataset_id: str) -> dict:
    ds = DATASETS_META.get(dataset_id)
    if not ds:
        return {}
    return {f["key"]: f["column"] for f in ds["fields"]}

# ── ROUTES ───────────────────────────────────────────────────

@app.get("/")
def root():
    return {"message": "InsightIQ API v2", "status": "running"}

@app.get("/api/health")
def health():
    try:
        conn = db.get_connection(); conn.close()
        return {"status": "ok", "database": "connected"}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/api/datasets")
def get_datasets():
    return list(DATASETS_META.values())

@app.get("/api/kpis")
def get_kpis():
    conn = db.get_connection()
    cur  = conn.cursor()
    try:
        kpis = {}
        cur.execute("SELECT SUM(revenue_ytd) FROM products")
        kpis["total_revenue"] = float(cur.fetchone()[0] or 0)
        cur.execute("SELECT AVG(avg_cart_size_lkr) FROM customers")
        kpis["avg_cart_size"] = float(cur.fetchone()[0] or 0)
        cur.execute("SELECT COUNT(*) FROM customers")
        kpis["active_customers"] = int(cur.fetchone()[0] or 0)
        cur.execute("SELECT COUNT(*) FROM products WHERE stock_level < reorder_point")
        kpis["low_stock_alerts"] = int(cur.fetchone()[0] or 0)
        cur.execute("SELECT category, SUM(revenue_ytd) AS rev FROM products GROUP BY category ORDER BY rev DESC LIMIT 1")
        row = cur.fetchone()
        kpis["top_category"] = row[0] if row else "N/A"
        kpis["top_category_revenue"] = float(row[1]) if row else 0
        cur.execute("SELECT SUM(revenue_ytd) FROM products WHERE is_halal = TRUE")
        halal_rev = float(cur.fetchone()[0] or 0)
        kpis["halal_revenue_pct"] = round((halal_rev / kpis["total_revenue"]) * 100, 1) if kpis["total_revenue"] > 0 else 0
        cur.execute("SELECT COUNT(*) FROM customers WHERE loyalty_tier = 'Gold'")
        kpis["gold_members"] = int(cur.fetchone()[0] or 0)
        cur.execute("SELECT COUNT(*) FROM transactions")
        kpis["total_transactions"] = int(cur.fetchone()[0] or 0)
        return kpis
    finally:
        cur.close(); conn.close()

@app.get("/api/dashboard/charts")
def get_dashboard_charts():
    conn = db.get_connection()
    cur  = conn.cursor()
    try:
        charts = {}
        cur.execute("""SELECT EXTRACT(MONTH FROM txn_date) AS mn, TO_CHAR(txn_date,'Mon') AS ml, ROUND(SUM(total_value_lkr)::numeric,2) AS rev FROM transactions GROUP BY mn, ml ORDER BY mn""")
        rows = cur.fetchall()
        charts["monthly_revenue"] = {"labels": [r[1] for r in rows], "data": [float(r[2]) for r in rows]}

        cur.execute("SELECT category, SUM(revenue_ytd) AS rev FROM products GROUP BY category ORDER BY rev DESC")
        rows = cur.fetchall()
        charts["category_revenue"] = {"labels": [r[0] for r in rows], "data": [float(r[1]) for r in rows]}

        cur.execute("""SELECT b.branch_name, ROUND(SUM(t.total_value_lkr)::numeric,2) AS rev FROM branches b LEFT JOIN transactions t ON t.branch_id=b.branch_id GROUP BY b.branch_id,b.branch_name ORDER BY rev DESC""")
        rows = cur.fetchall()
        charts["branch_revenue"] = {"labels": [r[0] for r in rows], "data": [float(r[1] or 0) for r in rows]}

        cur.execute("SELECT loyalty_tier, ROUND(AVG(avg_cart_size_lkr)::numeric,2) AS ac FROM customers GROUP BY loyalty_tier ORDER BY ac DESC")
        rows = cur.fetchall()
        charts["loyalty_cart"] = {"labels": [r[0] for r in rows], "data": [float(r[1]) for r in rows]}

        cur.execute("""SELECT c.area,
            ROUND(SUM(CASE WHEN p.is_halal THEN p.revenue_ytd ELSE 0 END)::numeric/1000000,2) AS halal_rev,
            ROUND(SUM(CASE WHEN NOT p.is_halal THEN p.revenue_ytd ELSE 0 END)::numeric/1000000,2) AS nonhalal_rev
            FROM products p CROSS JOIN (SELECT DISTINCT area FROM customers LIMIT 5) c
            GROUP BY c.area ORDER BY (halal_rev+nonhalal_rev) DESC""")
        rows = cur.fetchall()
        charts["halal_by_area"] = {"labels": [r[0] for r in rows], "halal": [float(r[1]) for r in rows], "non_halal": [float(r[2]) for r in rows]}
        return charts
    finally:
        cur.close(); conn.close()

@app.get("/api/top10")
def get_top10():
    conn = db.get_connection()
    cur  = conn.cursor()
    try:
        cur.execute("""SELECT ROW_NUMBER() OVER (ORDER BY p.revenue_ytd DESC) AS rank,
            p.product_name, p.category, p.brand, p.revenue_ytd, p.units_sold_ytd,
            p.is_halal, p.is_organic, p.avg_price_lkr, p.stock_level, p.reorder_point
            FROM products p ORDER BY p.revenue_ytd DESC LIMIT 10""")
        cols = [desc[0] for desc in cur.description]
        rows = [dict(zip(cols, row)) for row in cur.fetchall()]
        for r in rows:
            r["revenue_ytd"]   = float(r["revenue_ytd"])
            r["avg_price_lkr"] = float(r["avg_price_lkr"])
        return rows
    finally:
        cur.close(); conn.close()

# ── UPGRADED /api/query ──────────────────────────────────────
@app.post("/api/query")
def run_query(req: QueryRequest):
    ds = DATASETS_META.get(req.dataset)
    if not ds:
        raise HTTPException(status_code=400, detail=f"Unknown dataset: {req.dataset}")

    allowed = _get_allowed(req.dataset)
    all_fields = req.dimensions + req.measures
    if not all_fields:
        raise HTTPException(status_code=400, detail="Provide at least one dimension or measure")

    for f in all_fields:
        if f not in allowed:
            raise HTTPException(status_code=400, detail=f"Unknown field: {f}")

    conn = db.get_connection()
    try:
        result = qe.build_and_run(
            conn       = conn,
            table      = ds["table"],
            allowed_columns = allowed,
            dimensions = req.dimensions,
            measures   = req.measures,
            filters    = req.filters or [],
            limit      = req.limit or 50,
            order_by   = req.order_by,
            order_dir  = req.order_dir or "DESC",
        )
        result["dataset"] = req.dataset
        return result
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()

@app.get("/api/presets")
def get_presets():
    return [
        {"id":"top10_products",    "name":"Top 10 Best Sellers",       "icon":"🏆","color":"blue",
         "description":"Revenue + Units Sold grouped bar per category with brand breakdown",
         "tags":["Products","Revenue","Ranking"],
         "widgets":[
           {"type":"bar",  "dataset":"products",     "dimensions":["category"],           "measures":["revenue_ytd","units_sold_ytd"], "title":"Revenue & Units by Category"},
           {"type":"grid", "dataset":"products",     "dimensions":["product_name","category","brand"], "measures":["units_sold_ytd","revenue_ytd"], "title":"Product Table"}]},
        {"id":"monthly_trend",     "name":"Monthly Revenue Trend",     "icon":"📈","color":"green",
         "description":"Full-year trend line with discount tracked on dual axis",
         "tags":["Revenue","Trend","Monthly"],
         "widgets":[
           {"type":"line", "dataset":"transactions",  "dimensions":["txn_month"],          "measures":["total_value_lkr","discount_lkr"], "title":"Revenue vs Discount by Month"},
           {"type":"bar",  "dataset":"transactions",  "dimensions":["payment_method"],     "measures":["basket_count"],  "title":"Transactions by Payment"}]},
        {"id":"halal_intelligence","name":"Halal Demand Intelligence", "icon":"🕌","color":"amber",
         "description":"Halal vs non-halal revenue and avg cart by religion and area",
         "tags":["Halal","Customers","Area"],
         "widgets":[
           {"type":"bar",  "dataset":"customers",    "dimensions":["area","religion"],     "measures":["avg_cart_size_lkr"], "title":"Avg Cart by Area & Religion"},
           {"type":"pie",  "dataset":"customers",    "dimensions":["religion"],            "measures":["total_spend_ytd"],   "title":"Spend by Religion"}]},
        {"id":"customer_segments", "name":"Customer Segmentation",    "icon":"👥","color":"purple",
         "description":"Loyalty tier and age group vs cart size and visits — grouped bars",
         "tags":["Customers","Loyalty","Demographics"],
         "widgets":[
           {"type":"bar",  "dataset":"customers",    "dimensions":["loyalty_tier"],        "measures":["avg_cart_size_lkr","visits_per_month"], "title":"Cart & Visits by Tier"},
           {"type":"bar",  "dataset":"customers",    "dimensions":["age_group","gender"],  "measures":["avg_cart_size_lkr"], "title":"Cart by Age & Gender"},
           {"type":"grid", "dataset":"customers",    "dimensions":["loyalty_tier","gender","religion"], "measures":["avg_cart_size_lkr","total_spend_ytd"], "title":"Segment Summary"}]},
        {"id":"low_stock",         "name":"Low Stock Alerts",          "icon":"⚠️","color":"red",
         "description":"Stock on hand vs reorder point vs expiry risk per warehouse zone",
         "tags":["Inventory","Alerts","Stock"],
         "widgets":[
           {"type":"bar",  "dataset":"inventory",   "dimensions":["warehouse_zone"],      "measures":["quantity_on_hand","expiry_risk_units"], "title":"Stock vs Expiry by Zone"},
           {"type":"grid", "dataset":"inventory",   "dimensions":["sku","warehouse_zone"],"measures":["quantity_on_hand","days_of_supply","expiry_risk_units"], "title":"Inventory Status"}]},
        {"id":"branch_performance","name":"Branch Performance",        "icon":"📍","color":"cyan",
         "description":"Branch revenue vs footfall — spot which branches need attention",
         "tags":["Branches","Revenue","Locations"],
         "widgets":[
           {"type":"bar",  "dataset":"branches",    "dimensions":["city"],               "measures":["daily_footfall","branch_size_sqft"], "title":"Footfall & Size by City"},
           {"type":"grid", "dataset":"branches",    "dimensions":["branch_name","city","region"], "measures":["branch_size_sqft","daily_footfall"], "title":"Branch Details"}]},
        {"id":"promotion_roi",     "name":"Promotion ROI",             "icon":"🎯","color":"pink",
         "description":"Redemptions, revenue uplift and ROI — grouped by promo type and segment",
         "tags":["Promotions","ROI","Revenue"],
         "widgets":[
           {"type":"bar",  "dataset":"promotions",  "dimensions":["promo_type"],          "measures":["redemptions","roi_pct"], "title":"Redemptions & ROI by Type"},
           {"type":"bar",  "dataset":"promotions",  "dimensions":["target_segment"],      "measures":["revenue_uplift_lkr"],  "title":"Uplift by Segment"},
           {"type":"grid", "dataset":"promotions",  "dimensions":["promo_name","promo_type","target_segment"], "measures":["redemptions","revenue_uplift_lkr","roi_pct"], "title":"Promo Scorecard"}]},
        {"id":"supplier_scorecard","name":"Supplier Scorecard",        "icon":"🚛","color":"amber",
         "description":"On-time delivery % vs lead time per supplier country",
         "tags":["Suppliers","Delivery","Spend"],
         "widgets":[
           {"type":"bar",  "dataset":"suppliers",   "dimensions":["origin_country"],      "measures":["on_time_delivery_pct","lead_time_days"], "title":"Delivery % & Lead Time by Country"},
           {"type":"grid", "dataset":"suppliers",   "dimensions":["supplier_name","origin_country"], "measures":["lead_time_days","on_time_delivery_pct","spend_ytd"], "title":"Supplier Details"}]},
        {"id":"department_pl",     "name":"Department P&L",            "icon":"🏬","color":"green",
         "description":"Actual revenue vs target and shrinkage per department",
         "tags":["Departments","Revenue","Targets"],
         "widgets":[
           {"type":"bar",  "dataset":"departments", "dimensions":["dept_name"],           "measures":["actual_revenue","revenue_target"], "title":"Revenue vs Target"},
           {"type":"bar",  "dataset":"departments", "dimensions":["dept_name"],           "measures":["shrinkage_pct","staff_count"], "title":"Shrinkage & Staff"},
           {"type":"grid", "dataset":"departments", "dimensions":["dept_name","manager"], "measures":["staff_count","revenue_target","actual_revenue","shrinkage_pct"], "title":"Dept P&L Table"}]},
    ]