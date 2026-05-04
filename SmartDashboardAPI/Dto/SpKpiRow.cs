namespace SmartDashboardAPI.Dto
{
    public class SpKpiRow
    {
        public string MetricKey { get; set; }
        public decimal Value { get; set; }
        public string Format { get; set; }

    }
    public class SpChartRow2
    {
        public string ChartId { get; internal set; }
        public object Label { get; internal set; }
        public double Value1 { get; internal set; }
        public double Value2 { get; internal set; }
        public double Value3 { get; internal set; } 

        //public SpChartRow2(string chartId, object label, double value1, double value2, double value3)
        //{
        //    ChartId = chartId;
        //    Label = label;
        //    Value1 = value1;
        //    Value2 = value2;
        //    Value3 = value3;
        //}

    }
    public class SpTop10Row2
    {
        public object Rank { get; internal set; }
        public object ProductName { get; internal set; }
        public object Category { get; internal set; }
        public object Brand { get; internal set; }
        public object RevenueYtd { get; internal set; }
        public object GrossProfitYtd { get; internal set; }
        public object GpMarginPct { get; internal set; }
        public object UnitsSoldYtd { get; internal set; }
        public object IsHalal { get; internal set; }
        public object AvgPriceLkr { get; internal set; }
        public object StockLevel { get; internal set; }
    }
}
