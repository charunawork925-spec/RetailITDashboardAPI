using SmartDashboardAPI.Dto;

namespace SmartDashboardAPI.Interface
{
    public interface IDashboardService
    {
        Task<DashboardChartsResponse> GetDashboardChartsAsync();
        Task<List<Dictionary<string, object?>>> GetTop10Async();
    }
}
