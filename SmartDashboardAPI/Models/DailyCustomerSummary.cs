namespace SmartDashboardAPI.Models
{
    public class DailyCustomerSummary
    {
        public int SummaryId { get; set; }
        public DateTime SummaryDate { get; set; }
        public int? BranchId { get; set; }
        public string LoyaltyTier { get; set; }
        public string AgeGroup { get; set; }
        public string Religion { get; set; }
        public int CustomerCount { get; set; }
        public decimal TotalSpend { get; set; }
        public decimal AvgSpendPerCustomer { get; set; }
    }
}
