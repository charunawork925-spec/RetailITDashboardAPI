using Microsoft.EntityFrameworkCore;
using Npgsql;
using SmartDashboardAPI.Data;
using SmartDashboardAPI.Dto;
using SmartDashboardAPI.Interface;
using System.Data;
using System.Text.Json;

namespace SmartDashboardAPI.Repository
{
    public class DatabaseRepository : IDatabaseRepository
    {
        private readonly AppDbContext _context;
        public DatabaseRepository(AppDbContext context)
        {
            _context = context;
        }
        public  async Task<List<SpChartRow2>> ExecGetDashboardChartsAsync()
        {
            var results = new List<SpChartRow2>();
            var conn = _context.Database.GetDbConnection() as NpgsqlConnection
                ?? throw new InvalidOperationException();

            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            // cmd.CommandType = CommandType.StoredProcedure;
            // cmd.CommandText = "sp_get_dashboard_charts";
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT * FROM public.sp_get_dashboard_charts()";
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(new SpChartRow2
                {
                    ChartId = reader.GetString(0),
                    Label = reader.GetString(1),
                    Value1 = reader.IsDBNull(2) ? 0 : (double)reader.GetDecimal(2),
                    Value2 = reader.IsDBNull(3) ? 0 : (double)reader.GetDecimal(3),
                    Value3 = reader.IsDBNull(4) ? 0 : (double)reader.GetDecimal(4)
                });
                //results.Add(new SpChartRow2(
                //    reader.GetString(0),
                //    reader.GetString(1),
                //    reader.IsDBNull(2) ? 0m : reader.GetDecimal(2),
                //    reader.IsDBNull(3) ? 0m : reader.GetDecimal(3),
                //    reader.IsDBNull(4) ? 0m : reader.GetDecimal(4)
                //));
            }
            return results;
        }

        public async Task<List<SpKpiRow>> ExecGetKpisAsync()
        {
            var results = new List<SpKpiRow>();
            var conn = _context.Database.GetDbConnection() as NpgsqlConnection
                ?? throw new InvalidOperationException();

            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            //cmd.CommandType = CommandType.StoredProcedure;
            //cmd.CommandText = "sp_get_dashboard_kpis";
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT * FROM sp_get_dashboard_kpis()";

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(new SpKpiRow
                {
                    MetricKey = reader.GetString(0),
                    Value = reader.IsDBNull(1) ? 0m : reader.GetDecimal(1),
                    Format = reader.IsDBNull(2) ? "" : reader.GetString(2)
                });
            }
            return results;
        }

        public async Task<List<SpTop10Row2>> ExecGetTop10Async()
        {
            var results = new List<SpTop10Row2>();
            var conn = _context.Database.GetDbConnection() as NpgsqlConnection
                ?? throw new InvalidOperationException();

            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            //cmd.CommandType = CommandType.StoredProcedure;
            //cmd.CommandText = "sp_get_top10_products";
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT * FROM sp_get_top10_products";

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(new SpTop10Row2
                {
                    Rank = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    ProductName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Category = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Brand = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    RevenueYtd = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),
                    GrossProfitYtd = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5),
                    GpMarginPct = reader.IsDBNull(6) ? 0m : reader.GetDecimal(6),
                    UnitsSoldYtd = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                    IsHalal = !reader.IsDBNull(8) && reader.GetBoolean(8),
                    AvgPriceLkr = reader.IsDBNull(9) ? 0m : reader.GetDecimal(9),
                    StockLevel = reader.IsDBNull(10) ? 0 : reader.GetInt32(10)
                });
            }
            return results;
        }

        public async Task<List<Dictionary<string, object?>>> ExecQuerySpAsync(string table, string joinTable, string[] dimensions, string[] measures, FilterDto[] filters, int limit, string orderBy, string orderDir, Dictionary<string, string> aggFunctions)
        {
            var filtersJson = JsonSerializer.Serialize(filters);
            var aggFuncJson = JsonSerializer.Serialize(aggFunctions);

            //string joinTableParam = string.IsNullOrEmpty(joinTable) ? null : "transactions";


            //joinTable = "transactions"; // For testing, hardcode the join table to "transactions"

            var conn = _context.Database.GetDbConnection() as NpgsqlConnection
                ?? throw new InvalidOperationException("Expected NpgsqlConnection");

            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            //cmd.CommandType = CommandType.StoredProcedure;
            //cmd.CommandText = "sp_insightiq_query";

            //cmd.CommandType = CommandType.Text;
            //cmd.CommandText = "SELECT * FROM sp_insightiq_query()";

            //cmd.Parameters.AddWithValue("p_table", table);
            //cmd.Parameters.AddWithValue("p_join_table", (object?)joinTable ?? DBNull.Value);
            //cmd.Parameters.AddWithValue("p_dimensions", dimensions);
            //cmd.Parameters.AddWithValue("p_measures", measures);
            //cmd.Parameters.AddWithValue("p_filters_json", filtersJson);
            //cmd.Parameters.AddWithValue("p_limit", limit);
            //cmd.Parameters.AddWithValue("p_order_by", (object?)orderBy ?? DBNull.Value);
            //cmd.Parameters.AddWithValue("p_order_dir", orderDir);
            //cmd.Parameters.AddWithValue("p_agg_functions_json", aggFuncJson);

            cmd.CommandText = @"
                SELECT * FROM sp_insightiq_query(
                    @p_table,
                    @p_join_table,
                    @p_dimensions,
                    @p_measures,
                    @p_filters_json,
                    @p_limit,
                    @p_order_by,
                    @p_order_dir,
                    @p_agg_functions_json
                )";

            cmd.Parameters.AddWithValue("p_table", table);
            cmd.Parameters.AddWithValue("p_join_table", (object?)joinTable ?? DBNull.Value);

            cmd.Parameters.AddWithValue("p_dimensions", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text, dimensions);
            cmd.Parameters.AddWithValue("p_measures", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text, measures);

            cmd.Parameters.AddWithValue("p_filters_json", filtersJson);
            cmd.Parameters.AddWithValue("p_limit", limit);
            cmd.Parameters.AddWithValue("p_order_by", (object?)orderBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_order_dir", orderDir);
            cmd.Parameters.AddWithValue("p_agg_functions_json", aggFuncJson);

            var results = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var rowJson = reader.GetString(0);
                var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(rowJson);
                if (dict == null) continue;

                var row = new Dictionary<string, object?>();
                foreach (var kv in dict)
                {
                    row[kv.Key] = kv.Value.ValueKind switch
                    {
                        JsonValueKind.Number => kv.Value.TryGetDecimal(out var d) ? (object?)d : kv.Value.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Null => null,
                        _ => kv.Value.GetString()
                    };
                }
                results.Add(row);
            }
            return results;
        }

        public async Task<List<string>> GetDistinctValuesAsync(string tableName, string column, int limit)
        {
            var conn = _context.Database.GetDbConnection() as NpgsqlConnection
                   ?? throw new InvalidOperationException("Expected NpgsqlConnection");

            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            // Sanitize column name (ensure it's safe - you might want to validate against known columns)
            var sql = $@"
                SELECT DISTINCT 
                    CAST({column} AS TEXT) AS val 
                FROM {tableName} 
                WHERE {column} IS NOT NULL 
                ORDER BY val 
                LIMIT @limit";

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("limit", Math.Min(limit, 200));

            var results = new List<string>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var val = reader.GetString(0);
                if (!string.IsNullOrEmpty(val))
                    results.Add(val);
            }

            return results;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await _context.Database.OpenConnectionAsync();
                await _context.Database.CloseConnectionAsync();
                return true;
            }
            catch { return false; }
        }
    }
}
