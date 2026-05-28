using Admissions.Application.Analytics;
using Admissions.Application.Common;

namespace Admissions.Tests.Unit;

public class FeeSensitivityTests
{
    private static readonly AnalyticsOptions Opt = new();

    [Fact]
    public void Build_MockProgram1_PerfectNegativeCorrelation()
    {

        var result = FeeSensitivityCalculator.Build(1, "ქართული ფილოლოგია",
        [
            new TrendSnap(2023, 120, 98, 145, 2250),
            new TrendSnap(2024, 120, 108, 163, 2250),
            new TrendSnap(2025, 130, 115, 178, 2500),
        ], Opt);

        Assert.True(result.Indicative);
        Assert.False(result.InsufficientData);
        Assert.NotNull(result.PearsonCorrelation);
        Assert.Equal(-1, result.PearsonCorrelation!.Value, 4);
        Assert.Equal("negative", result.SlopeSign);
    }

    [Fact]
    public void Build_TwoSnapsOnly_OneDelta_Insufficient()
    {
        var result = FeeSensitivityCalculator.Build(1, "x",
        [
            new TrendSnap(2024, 100, 90, 50, 2000),
            new TrendSnap(2025, 100, 95, 60, 2200),
        ], Opt);

        Assert.True(result.InsufficientData);
        Assert.Null(result.PearsonCorrelation);
        Assert.Equal("flat", result.SlopeSign);
    }

    [Fact]
    public void Build_ConstantFees_NullCorrelation_Flat()
    {

        var result = FeeSensitivityCalculator.Build(1, "x",
        [
            new TrendSnap(2023, 100, 90, 50, 2000),
            new TrendSnap(2024, 100, 95, 60, 2000),
            new TrendSnap(2025, 100, 92, 75, 2000),
        ], Opt);

        Assert.False(result.InsufficientData);
        Assert.Null(result.PearsonCorrelation);
        Assert.Equal("flat", result.SlopeSign);
    }

    [Fact]
    public void Build_PositiveCorrelation_PositiveSign()
    {

        var result = FeeSensitivityCalculator.Build(1, "x",
        [
            new TrendSnap(2023, 100, 90, 100, 2000),
            new TrendSnap(2024, 100, 95, 110, 2200),
            new TrendSnap(2025, 100, 92, 132, 2640),
        ], Opt);

        Assert.Equal("positive", result.SlopeSign);
        Assert.Equal(1, result.PearsonCorrelation!.Value, 4);
    }
}
