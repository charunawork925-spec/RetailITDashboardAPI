using SmartDashboardAPI.Dto;

namespace SmartDashboardAPI.Interface
{
    public interface IQueryService
    {
        Task<QueryResponse> RunQueryAsync(QueryRequest req);

    }
}
