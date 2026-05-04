namespace SmartDashboardAPI.Models
{
    public class SpTop10Row
    {
        public int Rank { get; set; }
        public string ProductName { get; set; } = "";
        public string Category { get; set; } = "";
        public string Brand { get; set; } = "";
        public decimal RevenueYtd { get; set; }
        public decimal GrossProfitYtd { get; set; }
        public decimal GpMarginPct { get; set; }
        public int UnitsSoldYtd { get; set; }
        public bool IsHalal { get; set; }
        public decimal AvgPriceLkr { get; set; }
        public int StockLevel { get; set; }
    }
}
