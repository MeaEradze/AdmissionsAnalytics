using Admissions.Application.Common;
using Admissions.Application.Dtos;

namespace Admissions.Application.Analytics;

public static class ForecastBuilder
{
    public static ProgramForecastDto Build(
        int programId,
        string programName,
        IReadOnlyList<(int Year, int Enrolled)> history,
        AnalyticsOptions opt)
    {
        var ordered = history.OrderBy(h => h.Year).ToList();

        int point;
        int projectedYear;
        if (ordered.Count >= 2)
        {
            var xs = ordered.Select(h => (double)h.Year).ToList();
            var ys = ordered.Select(h => (double)h.Enrolled).ToList();
            var (slope, intercept) = Calc.Ols(xs, ys);
            projectedYear = ordered[^1].Year + 1;
            point = Math.Max(0, Calc.RoundToInt(slope * projectedYear + intercept));
        }
        else if (ordered.Count == 1)
        {
            projectedYear = ordered[0].Year + 1;
            point = Math.Max(0, ordered[0].Enrolled);
        }
        else
        {
            projectedYear = DateTime.UtcNow.Year + 1;
            point = 0;
        }

        return new ProgramForecastDto
        {
            ProgramId = programId,
            ProgramName = programName,
            MethodLabel = "წრფივი ტრენდის პროექცია",
            PointEstimate = point,
            LowerBound = Calc.RoundToInt(point * opt.ForecastLowerFactor),
            UpperBound = Calc.RoundToInt(point * opt.ForecastUpperFactor),
            ProjectedYear = projectedYear,
            HistoricalData = ordered
                .Select(h => new HistoricalPointDto { Year = h.Year, EnrolledCount = h.Enrolled })
                .ToList(),
        };
    }
}
