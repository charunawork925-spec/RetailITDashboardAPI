namespace SmartDashboardAPI.Dto
{
    public class ResponseDto
    {
        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public T? Data { get; set; }
            public List<string>? Errors { get; set; }
            public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        }

        public class SalesSummaryDto
        {
            public DateTime PeriodDate { get; set; }
            public string PeriodLabel { get; set; } = string.Empty;
            public int? BranchId { get; set; }
            public string BranchName { get; set; } = string.Empty;
            public string Region { get; set; } = string.Empty;
            public decimal TotalRevenue { get; set; }
            public long TotalTransactions { get; set; }
            public decimal AvgBasket { get; set; }
            public int UniqueCustomers { get; set; }
        }

        public class CategoryPerformanceDto
        {
            public string Category { get; set; } = string.Empty;
            public decimal TotalRevenue { get; set; }
            public long TotalUnitsSold { get; set; }
            public long TransactionCount { get; set; }
            public decimal AvgPrice { get; set; }
            public decimal RevenueShare { get; set; }
            public long RankPosition { get; set; }
        }

        public class CustomerInsightsDto
        {
            public string LoyaltyTier { get; set; } = string.Empty;
            public string AgeGroup { get; set; } = string.Empty;
            public string Religion { get; set; } = string.Empty;
            public long CustomerCount { get; set; }
            public decimal TotalSpend { get; set; }
            public decimal AvgSpendPerCustomer { get; set; }
            public decimal SpendShare { get; set; }
        }

        public class HourlyHeatmapDto
        {
            public DateTime SummaryDate { get; set; }
            public int HourOfDay { get; set; }
            public int? BranchId { get; set; }
            public string BranchName { get; set; } = string.Empty;
            public string DayOfWeek { get; set; } = string.Empty;
            public int TransactionCount { get; set; }
            public decimal Revenue { get; set; }
            public int ItemsSold { get; set; }
            public decimal AvgTransactionValue => TransactionCount > 0 ? Revenue / TransactionCount : 0;
        }

        public class ProductPerformanceDto
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string Brand { get; set; } = string.Empty;
            public int UnitsSold { get; set; }
            public decimal Revenue { get; set; }
            public decimal AvgPrice { get; set; }
            public int UniqueCustomers { get; set; }
            public decimal RevenueShare { get; set; }
        }

        public class LowStockAlertDto
        {
            public string ProductName { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string Sku { get; set; } = string.Empty;
            public int QuantityOnHand { get; set; }
            public int ReorderPoint { get; set; }
            public int DaysOfSupply { get; set; }
            public int ExpiryRiskUnits { get; set; }
            public string StockStatus { get; set; } = string.Empty;
        }

        public class PromotionEffectivenessDto
        {
            public int PromoId { get; set; }
            public string PromoName { get; set; } = string.Empty;
            public string PromoType { get; set; } = string.Empty;
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public decimal DiscountPct { get; set; }
            public int Redemptions { get; set; }
            public decimal RevenueUpliftLkr { get; set; }
            public decimal RoiPct { get; set; }
            public int AffectedTransactions { get; set; }
            public decimal AffectedRevenue { get; set; }
        }

        public class SalesSummaryResponse
        {
            public List<SalesSummaryDto> Data { get; set; } = new();
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string GroupBy { get; set; } = string.Empty;
            public decimal TotalRevenue { get; set; }
            public long TotalTransactions { get; set; }
            public decimal AverageDailyRevenue { get; set; }
        }

        public class CategoryPerformanceResponse
        {
            public List<CategoryPerformanceDto> Categories { get; set; } = new();
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public decimal GrandTotalRevenue { get; set; }
        }

        public class CustomerInsightsResponse
        {
            public List<CustomerInsightsDto> Segments { get; set; } = new();
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int TotalSegments { get; set; }
        }

        public class PagedResponse<T>
        {
            public List<T> Items { get; set; } = new();
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
            public bool HasNextPage => Page < TotalPages;
            public bool HasPreviousPage => Page > 1;
        }

        // Dataset metadata DTOs
        public class DatasetMeta
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Icon { get; set; } = string.Empty;
            public string TableName { get; set; } = string.Empty;
            public List<string> JoinTables { get; set; } = new();
            public List<FieldMeta> Fields { get; set; } = new();
            public List<string> DefaultDimensions { get; set; } = new();
            public Dictionary<string, List<FilterOption>> Filters { get; set; } = new();
        }

        public class FieldMeta
        {
            public string Name { get; set; } = string.Empty;
            public string Label { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty; // dim, msr, date, bool
            public string Column { get; set; } = string.Empty;

            public FieldMeta(string name, string label, string type, string column)
            {
                Name = name;
                Label = label;
                Type = type;
                Column = column;
            }
        }

        public class FilterOption
        {
            public string Label { get; set; }
            public object Value { get; set; }

            public FilterOption(string label, object value)
            {
                Label = label;
                Value = value;
            }
        }
    }
}
