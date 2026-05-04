using SmartDashboardAPI.Dto;
using SmartDashboardAPI.Interface;

namespace SmartDashboardAPI.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDatabaseRepository _repo;
        public DashboardService(IDatabaseRepository repo) { _repo = repo; }

        public async Task<DashboardChartsResponse> GetDashboardChartsAsync()
        {
            var allRows = await _repo.ExecGetDashboardChartsAsync();

            ChartSeries Series(string id) =>
                new(
                    allRows.Where(r => r.ChartId == id).Select(r => r.Label?.ToString() ?? string.Empty).ToList(),
                    allRows.Where(r => r.ChartId == id).Select(r => (double)r.Value1).ToList()
                );

            var gpRows = allRows.Where(r => r.ChartId == "revenue_vs_gp").ToList();
            var revenueGp = new DualChartSeries(
                gpRows.Select(r => r.Label?.ToString() ?? string.Empty).ToList(),
                gpRows.Select(r => (double)r.Value1).ToList(),
                gpRows.Select(r => (double)r.Value2).ToList()
            );

            return new DashboardChartsResponse(
                MonthlyRevenue: Series("monthly_revenue"),
                CategoryRevenue: Series("category_revenue"),
                BranchRevenue: Series("branch_revenue"),
                LoyaltyCart: Series("loyalty_cart"),
                RevenueVsGp: revenueGp
            );
        }

        public async Task<List<Dictionary<string, object?>>> GetTop10Async()
        {
            var rows = await _repo.ExecGetTop10Async();
            return rows.Select((r, i) => new Dictionary<string, object?>
            {
                ["rank"] = r.Rank,
                ["product_name"] = r.ProductName,
                ["category"] = r.Category,
                ["brand"] = r.Brand,
                ["revenue_ytd"] = r.RevenueYtd,
                ["gross_profit_ytd"] = r.GrossProfitYtd,
                ["gp_margin_pct"] = r.GpMarginPct,
                ["units_sold_ytd"] = r.UnitsSoldYtd,
                ["is_halal"] = r.IsHalal,
                ["avg_price_lkr"] = r.AvgPriceLkr,
                ["stock_level"] = r.StockLevel,
            }).ToList();
        }
    }
}
