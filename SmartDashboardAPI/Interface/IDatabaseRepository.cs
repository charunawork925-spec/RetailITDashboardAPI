using SmartDashboardAPI.Dto;
using SmartDashboardAPI.Models;

namespace SmartDashboardAPI.Interface
{
    public interface IDatabaseRepository
    {
        Task<List<Dictionary<string, object?>>> ExecQuerySpAsync(
        string table, string joinTable,
        string[] dimensions, string[] measures,
        FilterDto[] filters, int limit,
        string orderBy, string orderDir,
        Dictionary<string, string> aggFunctions);

        Task<List<Dto.SpKpiRow>> ExecGetKpisAsync();
        Task<List<SpChartRow2>> ExecGetDashboardChartsAsync();
        Task<List<SpTop10Row2>> ExecGetTop10Async();
        Task<List<string>> GetDistinctValuesAsync(string tableName, string column, int limit);
        Task<bool> TestConnectionAsync();
    }
}
