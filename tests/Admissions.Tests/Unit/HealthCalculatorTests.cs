using Admissions.Application.Analytics;
using Admissions.Application.Common;
using Admissions.Domain.Entities;
using Admissions.Domain.Enums;

namespace Admissions.Tests.Unit;

public class HealthCalculatorTests
{
    private static ProgramYearStat Stat(
        int programId, int announced, int enrolled, int fp, int? total, decimal fee) =>
        new()
        {
            ProgramId = programId,
            Year = 2025,
            AnnouncedPlaces = announced,
            EnrolledCount = enrolled,
            FirstPriorityCount = fp,
            TotalPriorityCount = total,
            AnnualFee = fee,
        };

    private static AnalyticsOptions MinMaxOpt => new() { NormalizationMode = "MinMax" };
    private static AnalyticsOptions PercentileOpt => new() { NormalizationMode = "Percentile" };

    [Fact]
    public void Compute_MinMax_HandComputedThreeProgramCohort()
    {

        List<ProgramYearStat> cohort =
        [
            Stat(1, 100, 50, 100, 400, 2000m),
            Stat(2, 100, 80, 300, 600, 3000m),
            Stat(3, 100, 100, 500, 666, 4000m),
        ];

        var result = HealthCalculator.Compute(cohort, new Dictionary<int, int>(), MinMaxOpt);

        var p2 = result[2];
        Assert.Equal(50, p2.DemandScore, 1);
        Assert.Equal(80, p2.FillRateScore, 1);
        Assert.Equal(50, p2.PriceScore, 1);
        Assert.Equal(49.9, p2.PriorityQualityScore, 1);

        Assert.Equal(57.5, p2.CompositeScore, 10);

        var p1 = result[1];
        Assert.Equal(0, p1.DemandScore);
        Assert.Equal(100, p1.PriceScore);
        var p3 = result[3];
        Assert.Equal(100, p3.DemandScore);
        Assert.Equal(0, p3.PriceScore);
    }

    [Fact]
    public void Compute_Percentile_MidRankScores()
    {
        List<ProgramYearStat> cohort =
        [
            Stat(1, 100, 50, 100, 400, 2000m),
            Stat(2, 100, 80, 300, 600, 3000m),
            Stat(3, 100, 100, 500, 666, 4000m),
        ];

        var result = HealthCalculator.Compute(cohort, new Dictionary<int, int>(), PercentileOpt);

        Assert.Equal(16.7, result[1].DemandScore, 1);
        Assert.Equal(50, result[2].DemandScore, 1);
        Assert.Equal(83.3, result[3].DemandScore, 1);
        Assert.Equal(100 - 16.7, result[1].PriceScore, 1);
    }

    [Fact]
    public void Compute_MissingTotalPriority_GetsNeutralScore()
    {
        List<ProgramYearStat> cohort =
        [
            Stat(1, 100, 50, 100, null, 2000m),
            Stat(2, 100, 80, 300, 600, 3000m),
        ];

        var result = HealthCalculator.Compute(cohort, new Dictionary<int, int>(), MinMaxOpt);
        Assert.Equal(50, result[1].PriorityQualityScore);
    }

    [Fact]
    public void Compute_ZeroAnnounced_FillScoreNeutralByDefault()
    {

        List<ProgramYearStat> cohort = [Stat(1, 0, 50, 100, 200, 2000m)];
        var result = HealthCalculator.Compute(cohort, new Dictionary<int, int>(), MinMaxOpt);
        Assert.Equal(0, result[1].FillRate);
        Assert.Equal(50, result[1].FillRateScore);
    }

    [Fact]
    public void Compute_ZeroAnnounced_FillScoreZeroWhenNeutralDisabled()
    {
        var opt = new AnalyticsOptions { NormalizationMode = "MinMax", TreatMissingDataAsNeutral = false };
        List<ProgramYearStat> cohort = [Stat(1, 0, 50, 100, 200, 2000m)];
        var result = HealthCalculator.Compute(cohort, new Dictionary<int, int>(), opt);
        Assert.Equal(0, result[1].FillRate);
        Assert.Equal(0, result[1].FillRateScore);
    }

    [Fact]
    public void Compute_MinMax_MissingFee_NeutralAndExcludedFromCohort()
    {

        List<ProgramYearStat> cohort =
        [
            Stat(1, 100, 50, 100, 400, 0m),
            Stat(2, 100, 80, 300, 600, 2000m),
            Stat(3, 100, 100, 500, 666, 4000m),
        ];

        var result = HealthCalculator.Compute(cohort, new Dictionary<int, int>(), MinMaxOpt);

        Assert.Equal(50, result[1].PriceScore);
        Assert.Equal(100, result[2].PriceScore);
        Assert.Equal(0, result[3].PriceScore);
    }

    [Fact]
    public void Compute_Percentile_MissingFee_NeutralAndExcludedFromCohort()
    {

        List<ProgramYearStat> cohort =
        [
            Stat(1, 100, 50, 100, 400, 0m),
            Stat(2, 100, 80, 300, 600, 2000m),
            Stat(3, 100, 100, 500, 666, 4000m),
        ];

        var result = HealthCalculator.Compute(cohort, new Dictionary<int, int>(), PercentileOpt);

        Assert.Equal(50, result[1].PriceScore);
        Assert.Equal(75, result[2].PriceScore, 1);
        Assert.Equal(25, result[3].PriceScore, 1);
    }

    [Fact]
    public void Compute_AllFeesMissing_EveryoneNeutralPrice()
    {
        List<ProgramYearStat> cohort =
        [
            Stat(1, 100, 50, 100, 400, 0m),
            Stat(2, 100, 80, 300, 600, 0m),
        ];

        var result = HealthCalculator.Compute(cohort, new Dictionary<int, int>(), MinMaxOpt);
        Assert.Equal(50, result[1].PriceScore);
        Assert.Equal(50, result[2].PriceScore);
    }

    [Fact]
    public void Compute_FillRateScore_ClampedAtHundred()
    {

        List<ProgramYearStat> cohort = [Stat(1, 100, 120, 100, 200, 2000m)];
        var result = HealthCalculator.Compute(cohort, new Dictionary<int, int>(), MinMaxOpt);
        Assert.Equal(100, result[1].FillRateScore);
        Assert.Equal(1.2, result[1].FillRate, 6);
    }

    [Fact]
    public void Category_GrowingNeedsThresholdAndPositiveTrend()
    {

        List<ProgramYearStat> cohort =
        [
            Stat(1, 100, 100, 500, 600, 1000m),
            Stat(2, 100, 20, 50, 400, 5000m),
        ];
        var prev = new Dictionary<int, int> { [1] = 400, [2] = 60 };

        var result = HealthCalculator.Compute(cohort, prev, MinMaxOpt);

        Assert.Equal(HealthCategory.Growing, result[1].Category);

        Assert.Equal(HealthCategory.Risky, result[2].Category);
    }

    [Fact]
    public void Category_HighCompositeWithoutTrend_IsStable()
    {
        List<ProgramYearStat> cohort =
        [
            Stat(1, 100, 100, 500, 600, 1000m),
            Stat(2, 100, 20, 50, 400, 5000m),
        ];

        var result = HealthCalculator.Compute(cohort, new Dictionary<int, int>(), MinMaxOpt);
        Assert.Equal(HealthCategory.Stable, result[1].Category);
    }

    [Fact]
    public void Category_DecliningDemandAndLowFill_IsRisky()
    {

        List<ProgramYearStat> cohort =
        [
            Stat(1, 100, 40, 300, 500, 2000m),
            Stat(2, 100, 90, 100, 300, 3000m),
            Stat(3, 100, 80, 500, 800, 4000m),
        ];
        var prev = new Dictionary<int, int> { [1] = 400 };

        var result = HealthCalculator.Compute(cohort, prev, MinMaxOpt);

        Assert.True(result[1].CompositeScore >= 45);
        Assert.Equal(HealthCategory.Risky, result[1].Category);
    }

    [Fact]
    public void Compute_EmptyCohort_EmptyResult()
    {
        var result = HealthCalculator.Compute([], new Dictionary<int, int>(), MinMaxOpt);
        Assert.Empty(result);
    }
}
