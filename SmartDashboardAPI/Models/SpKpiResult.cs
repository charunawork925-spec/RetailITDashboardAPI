namespace SmartDashboardAPI.Models
{
    public class SpKpiResult
    {
        public string MetricKey { get; set; } = "";
        public string MetricLabel { get; set; } = "";
        public decimal NumericValue { get; set; }
        public string TextValue { get; set; } = "";
    }
}
