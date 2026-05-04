namespace SmartDashboardAPI.Models
{
    public class HourlySalesSummary
    {
        public int SummaryId { get; set; }
        public DateTime SummaryDate { get; set; }
        public int HourOfDay { get; set; }
        public int? BranchId { get; set; }
        public string? DayOfWeek { get; set; }
        public int TransactionCount { get; set; }
        public decimal Revenue { get; set; }
        public int ItemsSold { get; set; }
    }
}
