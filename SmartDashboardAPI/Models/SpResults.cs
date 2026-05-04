namespace SmartDashboardAPI.Models
{
    //public class SpResults
    //{
    //}

    // ============================================================
    // InsightIQ — Stored Procedure Result Models
    // Models/SpResults.cs
    //
    // ONE place for all SP result classes.
    // Used by:
    //   - Data/InsightIqDbContext.cs   (registered as keyless DbSets)
    //   - Repository/DatabaseRepository.cs  (reader maps to these)
    //   - Services/Services.cs         (consumed in KpiService, DashboardService)
    // ============================================================


    // ── sp_insightiq_query result ─────────────────────────────────
    // Each row comes back from PostgreSQL as a JSON string.
    // DatabaseRepository deserialises it into Dictionary<string, object?>.
    // This class is registered as a keyless DbSet in case EF Core
    // FromSqlRaw is used instead of raw ADO.
    public class SpQueryResultRow
    {
        /// The raw JSON string for one row, e.g.
        /// {"category":"Dairy & Eggs","revenue_ytd":48200000}
        public string RowJson { get; set; } = string.Empty;
    }


    // ── sp_get_dashboard_kpis result ─────────────────────────────
    // Returns one row per KPI metric.
    // Columns: metric_key TEXT, numeric_value NUMERIC, text_value TEXT
    //
    // Example rows returned by the SP:
    //   ("total_revenue",       284700000, NULL)
    //   ("top_category",        NULL,      "Dairy & Eggs")
    //   ("gold_members",        8941,      NULL)
    public class SpKpiRow
    {
        /// Unique key identifying the KPI, e.g. "total_revenue", "gold_members"
        public string MetricKey { get; set; } = string.Empty;

        /// Numeric value — used for revenue, counts, percentages.
        /// NULL for text-only metrics like top_category.
        public decimal NumericValue { get; set; }

        /// Text value — used for string metrics like top_category.
        /// NULL for numeric-only metrics.
        public string? TextValue { get; set; }
    }


    // ── sp_get_dashboard_charts result ───────────────────────────
    // Returns rows for all 5 dashboard charts in one call.
    // Columns: chart_id TEXT, label TEXT, value1 NUMERIC, value2 NUMERIC, value3 NUMERIC
    //
    // chart_id identifies which chart the row belongs to:
    //   "monthly_revenue"  — value1 = revenue
    //   "category_revenue" — value1 = revenue
    //   "branch_revenue"   — value1 = revenue
    //   "loyalty_cart"     — value1 = avg cart size
    //   "revenue_vs_gp"    — value1 = revenue, value2 = gross profit
    //
    // value2 and value3 are 0 for charts that only need one series.
    //public class SpChartRow
    //{
    //    /// Which chart this row belongs to
    //    public string ChartId { get; set; } = string.Empty;

    //    /// X-axis label, e.g. "Jan", "Dairy & Eggs", "FreshMart Colombo 03"
    //    public string Label { get; set; } = string.Empty;

    //    /// Primary series value (always populated)
    //    public decimal Value1 { get; set; }

    //    /// Secondary series value (e.g. Gross Profit in revenue_vs_gp chart)
    //    public decimal Value2 { get; set; }

    //    /// Tertiary series value (reserved for future 3-series charts)
    //    public decimal Value3 { get; set; }
    //}


    //// ── sp_get_top10_products result ──────────────────────────────
    //// Returns exactly 10 rows ordered by revenue descending.
    //// Columns match the SELECT list in the stored procedure.
    //public class SpTop10Row
    //{
    //    /// Position 1–10
    //    public int Rank { get; set; }

    //    public string ProductName { get; set; } = string.Empty;
    //    public string Category { get; set; } = string.Empty;
    //    public string Brand { get; set; } = string.Empty;

    //    /// Total revenue year-to-date in LKR
    //    public decimal RevenueYtd { get; set; }

    //    /// Total gross profit year-to-date in LKR
    //    public decimal GrossProfitYtd { get; set; }

    //    /// Gross profit margin as a percentage, e.g. 35.0
    //    public decimal GpMarginPct { get; set; }

    //    /// Total units sold year-to-date
    //    public int UnitsSoldYtd { get; set; }

    //    /// Whether the product is Halal certified
    //    public bool IsHalal { get; set; }

    //    /// Average selling price per unit in LKR
    //    public decimal AvgPriceLkr { get; set; }

    //    /// Current stock level in units
    //    public int StockLevel { get; set; }
    //}
}
