using SmartDashboardAPI.Dto;

namespace SmartDashboardAPI.Models
{
    public class QueryRequest
    {
        //     string Dataset,
        //List<string>        Dimensions,
        // List<string> Measures,
        // List<FilterDto>?    Filters,
        // int? Limit,
        // string? OrderBy,
        // string? OrderDir,
        // string? JoinDataset,
        // Dictionary<string, string>? AggFunctions

        public record wdwdq(
    string Dataset,
    List<string> Dimensions,
    List<string> Measures,
    List<FilterDto>? Filters,
    int? Limit,
    string? OrderBy,
    string? OrderDir,
    string? JoinDataset,
    Dictionary<string, string>? AggFunctions
);
    }
}
