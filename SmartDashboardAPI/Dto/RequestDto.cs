namespace SmartDashboardAPI.Dto
{
    public class RequestDto
    {
        public class DateRangeRequest
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int[]? BranchIds { get; set; }
            public string? GroupBy { get; set; } = "day"; // day, week, month, quarter
        }

        public class FilterRequest
        {
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public int[]? BranchIds { get; set; }
            public string[]? Categories { get; set; }
            public string[]? LoyaltyTiers { get; set; }
            public string[]? PaymentMethods { get; set; }
            public string[]? TimeOfDay { get; set; }
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 50;
        }

        public class DatasetQueryRequest
        {
            public string DatasetId { get; set; }
            public List<string> Dimensions { get; set; } = new();
            public List<string> Measures { get; set; } = new();
            public Dictionary<string, List<object>> Filters { get; set; } = new();
            public string? SortBy { get; set; }
            public string SortOrder { get; set; } = "ASC";
            public int Limit { get; set; } = 1000;
            public int Offset { get; set; } = 0;
        }

        public class SummaryGenerationRequest
        {
            public DateTime? TargetDate { get; set; }
            public bool BackfillHistorical { get; set; } = false;
            public DateTime? BackfillStartDate { get; set; }
            public DateTime? BackfillEndDate { get; set; }
        }
    }
}
