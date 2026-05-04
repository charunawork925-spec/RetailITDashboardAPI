using SmartDashboardAPI.Dto;

namespace SmartDashboardAPI.Interface
{
    public interface IChartBuilderService
    {
        ChartPayload Build(
       List<Dictionary<string, object?>> rows,
       List<string> dimensions,
       List<string> measures);
    }
}
