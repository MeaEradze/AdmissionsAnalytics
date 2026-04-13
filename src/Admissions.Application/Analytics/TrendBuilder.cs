using Admissions.Application.Common;
using Admissions.Application.Dtos;
using Admissions.Domain.Enums;

namespace Admissions.Application.Analytics;

public sealed record TrendSnap(int Year, int Announced, int Enrolled, int FirstPriority, double Fee);

public static class TrendBuilder
{
    public static TrendResultDto Build(
        int entityId,
        string entityName,
        IReadOnlyList<TrendSnap> snaps,
        AnalyticsOptions opt)
    {
        var deltas = new List<TrendPointDto>();
        for (int i = 1; i < snaps.Count; i++)
        {
            var prev = snaps[i - 1];
            var cur = snaps[i];

            double? demandDelta = prev.FirstPriority > 0
                ? (double)(cur.FirstPriority - prev.FirstPriority) / prev.FirstPriority
                : null;
            double? fillDelta = prev.Announced > 0 && cur.Announced > 0
                ? (double)cur.Enrolled / cur.Announced - (double)prev.Enrolled / prev.Announced
                : null;
            double? feeDelta = prev.Fee > 0
                ? (cur.Fee - prev.Fee) / prev.Fee
                : null;

            deltas.Add(new TrendPointDto
            {
                Year = cur.Year,
                DemandDelta = demandDelta,
                FillDelta = fillDelta,
                FeeDelta = feeDelta,
            });
        }

        double? demandCagr = null;
        if (snaps.Count > 1 && snaps[0].FirstPriority > 0)
        {
            int years = snaps.Count - 1;
            demandCagr = Math.Pow(
                (double)snaps[^1].FirstPriority / snaps[0].FirstPriority,
                1.0 / years) - 1;
        }

        double avgDelta = deltas.Count > 0
            ? deltas.Sum(d => d.DemandDelta ?? 0) / deltas.Count
            : 0;

        var label = avgDelta > opt.TrendGrowingCutoff
            ? TrendDirection.Growing
            : avgDelta < opt.TrendDecliningCutoff
                ? TrendDirection.Declining
                : TrendDirection.Stable;

        return new TrendResultDto
        {
            EntityId = entityId,
            EntityName = entityName,
            DemandCagr = demandCagr,
            DemandTrendLabel = label,
            YoYDeltas = deltas,
            YearSeries = snaps
                .Select(s => new TrendYearPointDto
                {
                    Year = s.Year,
                    AnnouncedPlaces = s.Announced,
                    EnrolledCount = s.Enrolled,
                    FirstPriorityCount = s.FirstPriority,
                    AnnualFee = s.Fee,
                })
                .ToList(),
        };
    }
}
