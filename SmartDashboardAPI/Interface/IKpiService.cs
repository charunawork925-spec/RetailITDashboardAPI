using SmartDashboardAPI.Dto;

namespace SmartDashboardAPI.Interface
{
    public interface IKpiService
    {
        Task<KpiResponse> GetKpisAsync();

    }
}
