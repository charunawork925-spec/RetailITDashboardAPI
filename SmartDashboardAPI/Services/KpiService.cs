using SmartDashboardAPI.Dto;
using SmartDashboardAPI.Interface;

namespace SmartDashboardAPI.Services
{
    public class KpiService : IKpiService
    {
        private readonly IDatabaseRepository _repo;
        public KpiService(IDatabaseRepository repo) { _repo = repo; }

        public async Task<KpiResponse> GetKpisAsync()
        {
            var rows = await _repo.ExecGetKpisAsync();
            var kpis = rows.ToDictionary(r => r.MetricKey, r => r);

            //double Get(string key) => kpis.TryGetValue(key, out var r) ? (double)r.NumericValue : 0;
            //string GetStr(string key) => kpis.TryGetValue(key, out var r) ? r.TextValue : "";
            //int GetInt(string key) => (int)Get(key);
            double Get(string key) =>
    kpis.TryGetValue(key, out var r) ? (double)r.Value : 0;

            string GetStr(string key) =>
                kpis.TryGetValue(key, out var r)
                    ? (!string.IsNullOrEmpty(r.Format) ? r.Format : r.Value.ToString())
                    : "";

            int GetInt(string key) => (int)Get(key);

            return new KpiResponse(
                TotalRevenue: Get("total_revenue"),
                TotalGrossProfit: Get("total_gross_profit"),
                OverallGpMargin: Get("overall_gp_margin"),
                AvgCartSize: Get("avg_cart_size"),
                ActiveCustomers: GetInt("active_customers"),
                LowStockAlerts: GetInt("low_stock_alerts"),
                TopCategory: GetStr("top_category"),
                TopCategoryRevenue: Get("top_category_revenue"),
                HalalRevenuePct: Get("halal_revenue_pct"),
                GoldMembers: GetInt("gold_members"),
                TotalTransactions: GetInt("total_transactions")
            );
        }
    }
}
