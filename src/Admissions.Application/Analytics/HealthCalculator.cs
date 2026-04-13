using Admissions.Application.Common;
using Admissions.Domain.Entities;
using Admissions.Domain.Enums;

namespace Admissions.Application.Analytics;

public sealed record HealthResult(
    double DemandScore,
    double FillRateScore,
    double PriorityQualityScore,
    double PriceScore,
    double CompositeScore,
    HealthCategory Category,
    double FillRate);

public static class HealthCalculator
{

    public static Dictionary<int, HealthResult> Compute(
        IReadOnlyList<ProgramYearStat> cohort,
        IReadOnlyDictionary<int, int> previousYearFirstPriority,
        AnalyticsOptions opt)
    {
        var results = new Dictionary<int, HealthResult>(cohort.Count);
        if (cohort.Count == 0)
        {
            return results;
        }

        bool percentile = !string.Equals(opt.NormalizationMode, "MinMax", StringComparison.OrdinalIgnoreCase);
        bool neutralMissing = opt.TreatMissingDataAsNeutral;

        var fpSorted = cohort.Select(s => (double)s.FirstPriorityCount).OrderBy(v => v).ToList();

        var feeSorted = cohort
            .Where(s => !neutralMissing || s.AnnualFee > 0)
            .Select(s => (double)s.AnnualFee)
            .OrderBy(v => v)
            .ToList();
        var pqSorted = cohort
            .Where(s => s.TotalPriorityCount is > 0)
            .Select(s => (double)s.FirstPriorityCount / s.TotalPriorityCount!.Value)
            .OrderBy(v => v)
            .ToList();

        double fpMin = fpSorted[0], fpMax = fpSorted[^1];
        double feeMin = feeSorted.Count > 0 ? feeSorted[0] : 0;
        double feeMax = feeSorted.Count > 0 ? feeSorted[^1] : 0;
        double pqMin = pqSorted.Count > 0 ? pqSorted[0] : 0;
        double pqMax = pqSorted.Count > 0 ? pqSorted[^1] : 0;

        double Normalize(IReadOnlyList<double> sorted, double min, double max, double value) =>
            percentile
                ? Calc.PercentileRank(sorted, value)
                : Calc.MinMaxScore(value, min, max, opt.NeutralScore);

        foreach (var stat in cohort)
        {
            double fillRate = stat.AnnouncedPlaces > 0
                ? (double)stat.EnrolledCount / stat.AnnouncedPlaces
                : 0;

            double fillScore = stat.AnnouncedPlaces > 0
                ? Math.Clamp(fillRate, 0, 1) * 100
                : neutralMissing ? opt.NeutralScore : 0;

            double demandScore = Normalize(fpSorted, fpMin, fpMax, stat.FirstPriorityCount);

            double pqScore = stat.TotalPriorityCount is > 0
                ? Normalize(pqSorted, pqMin, pqMax,
                    (double)stat.FirstPriorityCount / stat.TotalPriorityCount.Value)
                : opt.NeutralScore;

            double priceScore = neutralMissing && stat.AnnualFee <= 0
                ? opt.NeutralScore
                : 100 - Normalize(feeSorted, feeMin, feeMax, (double)stat.AnnualFee);

            double composite =
                opt.DemandWeight * demandScore +
                opt.FillRateWeight * fillScore +
                opt.PriorityQualityWeight * pqScore +
                opt.PriceWeight * priceScore;

            composite = Calc.Round(composite, 1);

            bool trendPositive = false, trendNegative = false;
            if (previousYearFirstPriority.TryGetValue(stat.ProgramId, out int prevFp))
            {
                trendPositive = stat.FirstPriorityCount > prevFp;
                trendNegative = stat.FirstPriorityCount < prevFp;
            }

            HealthCategory category;
            if (composite >= opt.GrowingThreshold && trendPositive)
            {
                category = HealthCategory.Growing;
            }
            else if (composite < opt.RiskyThreshold ||
                     (trendNegative && fillRate < opt.LowFillRateThreshold))
            {
                category = HealthCategory.Risky;
            }
            else
            {
                category = HealthCategory.Stable;
            }

            results[stat.ProgramId] = new HealthResult(
                Calc.Round(demandScore, 1),
                Calc.Round(fillScore, 1),
                Calc.Round(pqScore, 1),
                Calc.Round(priceScore, 1),
                composite,
                category,
                fillRate);
        }

        return results;
    }
}
