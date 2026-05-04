namespace SmartDashboardAPI.Models
{
    public class DailyCategorySummary
    {
        public int SummaryId { get; set; }
        public DateTime SummaryDate { get; set; }
        public int? BranchId { get; set; }
        public string Category { get; set; }
        public decimal Revenue { get; set; }
        public int UnitsSold { get; set; }
        public int TransactionCount { get; set; }
        public decimal AvgPrice { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
