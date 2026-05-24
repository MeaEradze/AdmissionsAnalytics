using Admissions.Application.Analytics;
using Admissions.Application.Common;
using Admissions.Domain.Enums;

namespace Admissions.Tests.Unit;

public class TrendBuilderTests
{
    private static readonly AnalyticsOptions Opt = new();

    private static readonly List<TrendSnap> Snaps =
    [
        new(2023, 120, 98, 145, 2250),
        new(2024, 120, 108, 163, 2250),
        new(2025, 130, 115, 178, 2500),
    ];

    [Fact]
    public void Build_YoYDeltas_HandComputed()
    {
        var result = TrendBuilder.Build(1, "ქართული ფილოლოგია", Snaps, Opt);

        Assert.Equal(2, result.YoYDeltas.Count);

        var d2024 = result.YoYDeltas[0];
        Assert.Equal(2024, d2024.Year);
        Assert.Equal((163.0 - 145) / 145, d2024.DemandDelta!.Value, 6);
        Assert.Equal(108.0 / 120 - 98.0 / 120, d2024.FillDelta!.Value, 6);
        Assert.Equal(0, d2024.FeeDelta!.Value, 6);

        var d2025 = result.YoYDeltas[1];
        Assert.Equal(2025, d2025.Year);
        Assert.Equal((178.0 - 163) / 163, d2025.DemandDelta!.Value, 6);
        Assert.Equal(115.0 / 130 - 108.0 / 120, d2025.FillDelta!.Value, 6);
        Assert.Equal((2500.0 - 2250) / 2250, d2025.FeeDelta!.Value, 6);
    }

    [Fact]
    public void Build_Cagr_HandComputed()
    {
        var result = TrendBuilder.Build(1, "x", Snaps, Opt);
        Assert.NotNull(result.DemandCagr);
        Assert.Equal(Math.Pow(178.0 / 145, 1.0 / 2) - 1, result.DemandCagr!.Value, 6);
    }

    [Fact]
    public void Build_GrowingLabel_WhenMeanDeltaAboveCutoff()
    {
        var result = TrendBuilder.Build(1, "x", Snaps, Opt);

        Assert.Equal(TrendDirection.Growing, result.DemandTrendLabel);
    }

    [Fact]
    public void Build_DecliningLabel_WhenMeanDeltaBelowCutoff()
    {
        List<TrendSnap> declining =
        [
            new(2023, 60, 45, 38, 3500),
            new(2024, 60, 38, 32, 3800),
            new(2025, 60, 28, 25, 4000),
        ];
        var result = TrendBuilder.Build(9, "x", declining, Opt);
        Assert.Equal(TrendDirection.Declining, result.DemandTrendLabel);
    }

    [Fact]
    public void Build_SingleYear_NullCagrEmptyDeltasStable()
    {
        var result = TrendBuilder.Build(1, "x", [new TrendSnap(2025, 100, 90, 50, 2000)], Opt);
        Assert.Null(result.DemandCagr);
        Assert.Empty(result.YoYDeltas);
        Assert.Equal(TrendDirection.Stable, result.DemandTrendLabel);
    }

    [Fact]
    public void Build_ZeroFirstPriorityBase_OmitsDemandDeltaAndCagr()
    {
        List<TrendSnap> snaps =
        [
            new(2023, 100, 50, 0, 2000),
            new(2024, 100, 60, 10, 2000),
        ];
        var result = TrendBuilder.Build(1, "x", snaps, Opt);
        Assert.Null(result.DemandCagr);
        Assert.Null(result.YoYDeltas[0].DemandDelta);
        Assert.NotNull(result.YoYDeltas[0].FillDelta);
    }
}
