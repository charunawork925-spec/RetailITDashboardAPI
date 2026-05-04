using SmartDashboardAPI.Dto;

namespace SmartDashboardAPI.Interface
{
    public interface IDatasetService
    {
        List<DatasetMeta> GetAllDatasets();
        DatasetMeta? GetDataset(string id);
        Task<List<string>> GetDistinctValuesAsync(string datasetId, string column, int limit);


    }
}
