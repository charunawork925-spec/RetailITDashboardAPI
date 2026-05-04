namespace SmartDashboardAPI.Dto
{
    public record QueryRequest(
        string Dataset,
        List<string> Dimensions,
        List<string> Measures,
        List<FilterDto>? Filters,
        int? Limit,
        string? OrderBy,
        string? OrderDir,
        string? JoinDataset,
        Dictionary<string, string>? AggFunctions
    );

    public record FilterDto(
        string field,
        string op,
        object? value
    );

    // ── OUTBOUND ─────────────────────────────────────────────────

    public record QueryResponse(
        string Dataset,
        List<string> Columns,
        List<Dictionary<string, object?>> Rows,
        int Count,
        ChartPayload Chart
    );

    public record ChartPayload(
        List<string> Labels,
        List<ChartDataset> Datasets,
        bool DualAxis,
        string? Warning
    );

    public record ChartDataset(
        string Label,
        List<double> Data,
        string BackgroundColor,
        string BorderColor,
        int BorderWidth,
        int BorderRadius,
        string? YAxisId
    );

    public record KpiResponse(
        double TotalRevenue,
        double TotalGrossProfit,
        double OverallGpMargin,
        double AvgCartSize,
        int ActiveCustomers,
        int LowStockAlerts,
        string TopCategory,
        double TopCategoryRevenue,
        double HalalRevenuePct,
        int GoldMembers,
        int TotalTransactions
    );

    public record DashboardChartsResponse(
        ChartSeries MonthlyRevenue,
        ChartSeries CategoryRevenue,
        ChartSeries BranchRevenue,
        ChartSeries LoyaltyCart,
        DualChartSeries RevenueVsGp
    );

    public record ChartSeries(
        List<string> Labels,
        List<double> Data
    );

    public record DualChartSeries(
        List<string> Labels,
        List<double> Revenue,
        List<double> Gp
    );

    // ── DATASET METADATA ─────────────────────────────────────────

    public record DatasetMeta(
        string Id,
        string Label,
        string Icon,
        string Table,
        List<string> JoinTargets,
        List<FieldMeta> Fields,
        List<string> FilterFields,
        Dictionary<string, List<FilterOption>> FilterOptions
    );

    public record FieldMeta(
        string Key,
        string Label,
        string Type,    // dim | msr | date | bool
        string Column
    );

    public record FilterOption(
        string Label,
        object Value
    );
}
