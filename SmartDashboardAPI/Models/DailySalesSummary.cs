namespace SmartDashboardAPI.Models
{
    public class DailySalesSummary
    {
        public int SummaryId { get; set; }
        public DateTime SummaryDate { get; set; }
        public int? BranchId { get; set; }
        public string BranchName { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalDiscount { get; set; }
        public int TotalItemsSold { get; set; }
        public int TransactionCount { get; set; }
        public int UniqueCustomers { get; set; }
        public decimal AvgBasketValue { get; set; }
        public decimal AvgItemsPerBasket { get; set; }
        public decimal CashRevenue { get; set; }
        public decimal CardRevenue { get; set; }
        public decimal QrRevenue { get; set; }
        public decimal VoucherRevenue { get; set; }
        public decimal MorningRevenue { get; set; }
        public decimal AfternoonRevenue { get; set; }
        public decimal EveningRevenue { get; set; }
        public decimal NightRevenue { get; set; }
    }
}
