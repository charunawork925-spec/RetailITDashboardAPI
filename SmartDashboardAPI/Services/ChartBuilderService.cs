using SmartDashboardAPI.Dto;
using SmartDashboardAPI.Interface;

namespace SmartDashboardAPI.Services
{
    public class ChartBuilderService : IChartBuilderService
    {
        public ChartPayload Build(
      List<Dictionary<string, object?>> rows,
      List<string> dimensions,
      List<string> measures)
        {
            if (rows.Count == 0)
                return new ChartPayload([], [], false, null);

            // Composite X labels from all dimensions joined by " / "
            var labels = rows.Select(r =>
                string.Join(" / ", dimensions
                    .Select(d => r.TryGetValue(d, out var v) ? v?.ToString() ?? "" : "")
                    .Where(s => !string.IsNullOrEmpty(s)))
            ).ToList();

            // Detect dual axis need (order-of-magnitude > 1.5 apart)
            bool dualAxis = false;
            if (measures.Count >= 2)
                dualAxis = NeedsDualAxis(rows, measures);

            var datasets = measures.Select((m, i) =>
            {
                var data = rows.Select(r =>
                    r.TryGetValue(m, out var v) && v is not null
                        ? Convert.ToDouble(v) : 0.0
                ).ToList();

                return new ChartDataset(
                    Label: m,
                    Data: data,
                    BackgroundColor: Palette.Get(i) + "cc",
                    BorderColor: Palette.Get(i),
                    BorderWidth: 2,
                    BorderRadius: 5,
                    YAxisId: dualAxis ? (i == 0 ? "y" : "y1") : null
                );
            }).ToList();

            string? warning = dimensions.Count > 2 && measures.Count > 2
                ? "Complex combination — consider splitting into separate widgets."
                : null;

            return new ChartPayload(labels, datasets, dualAxis, warning);
        }

        private static bool NeedsDualAxis(
            List<Dictionary<string, object?>> rows,
            List<string> measures)
        {
            static double Magnitude(List<Dictionary<string, object?>> rows, string m)
            {
                var vals = rows
                    .Select(r => r.TryGetValue(m, out var v) && v is not null ? Math.Abs(Convert.ToDouble(v)) : 0.0)
                    .Where(v => v > 0).ToList();
                if (vals.Count == 0) return 0;
                return Math.Log10(vals.Average() + 1);
            }

            double baseMag = Magnitude(rows, measures[0]);
            return measures.Skip(1).Any(m => Math.Abs(Magnitude(rows, m) - baseMag) > 1.5);
        }
    }
}
