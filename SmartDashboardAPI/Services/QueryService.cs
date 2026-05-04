using SmartDashboardAPI.Dto;
using SmartDashboardAPI.Interface;

namespace SmartDashboardAPI.Services
{
    public class QueryService : IQueryService
    {
        private readonly IDatabaseRepository _repo;
        private readonly IDatasetService _ds;
        private readonly IChartBuilderService _chart;

        public QueryService(IDatabaseRepository repo, IDatasetService ds, IChartBuilderService chart)
        {
            _repo = repo;
            _ds = ds;
            _chart = chart;
        }
        public async Task<QueryResponse> RunQueryAsync(QueryRequest req)
        {
            var meta = _ds.GetDataset(req.Dataset)
                ?? throw new ArgumentException($"Unknown dataset: {req.Dataset}");

            // Validate fields
            var allowed = meta.Fields.ToDictionary(f => f.Key);
            foreach (var f in req.Dimensions.Concat(req.Measures))
                if (!allowed.ContainsKey(f))
                    throw new ArgumentException($"Unknown field: {f}");

            var rows = await _repo.ExecQuerySpAsync(
                table: meta.Table,
                joinTable: req.JoinDataset ?? "",
                dimensions: [.. req.Dimensions],
                measures: [.. req.Measures],
                filters: [.. (req.Filters ?? [])],
                limit: Math.Min(req.Limit ?? 50, 1000),
                orderBy: req.OrderBy ?? (req.Measures.Count > 0 ? req.Measures[0] : ""),
                orderDir: req.OrderDir ?? "DESC",
                aggFunctions: req.AggFunctions ?? []
            );

            //var columns = rows.Count > 0
            //    ? [.. rows[0].Keys]
            //    : [.. req.Dimensions, .. req.Measures];
            List<string> columns = rows.Count > 0
                                    ? rows[0].Keys.ToList()
                                    : req.Dimensions.Concat(req.Measures).ToList();


            var chart = _chart.Build(rows, req.Dimensions, req.Measures);

            return new QueryResponse(req.Dataset, columns, rows, rows.Count, chart);
        }
    }
}
