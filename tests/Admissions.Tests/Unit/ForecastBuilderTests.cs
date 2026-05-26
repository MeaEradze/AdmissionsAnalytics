using Admissions.Application.Analytics;
using Admissions.Application.Common;

namespace Admissions.Tests.Unit;

public class ForecastBuilderTests
{
    private static readonly AnalyticsOptions Opt = new();

    [Fact]
    public void Build_OlsProjection_HandComputed()
    {

        var result = ForecastBuilder.Build(
            1, "ქართული ფილოლოგია",
            [(2023, 98), (2024, 108), (2025, 115)],
            Opt);

        Assert.Equal("წრფივი ტრენდის პროექცია", result.MethodLabel);
        Assert.Equal(2026, result.ProjectedYear);
        Assert.Equal(124, result.PointEstimate);
        Assert.Equal(112, result.LowerBound);
        Assert.Equal(136, result.UpperBound);
        Assert.Equal(3, result.HistoricalData.Count);
        Assert.Equal(2023, result.HistoricalData[0].Year);
    }

    [Fact]
    public void Build_DecliningSeries_ClampsAtZero()
    {
        var result = ForecastBuilder.Build(1, "x", [(2023, 30), (2024, 10)], Opt);

        Assert.Equal(0, result.PointEstimate);
        Assert.Equal(0, result.LowerBound);
        Assert.Equal(0, result.UpperBound);
        Assert.Equal(2025, result.ProjectedYear);
    }

    [Fact]
    public void Build_SinglePoint_ProjectsFlat()
    {
        var result = ForecastBuilder.Build(1, "x", [(2025, 200)], Opt);
        Assert.Equal(200, result.PointEstimate);
        Assert.Equal(2026, result.ProjectedYear);
    }

    [Fact]
    public void Build_NoHistory_ZeroEstimate()
    {
        var result = ForecastBuilder.Build(1, "x", [], Opt);
        Assert.Equal(0, result.PointEstimate);
        Assert.Empty(result.HistoricalData);
    }
}
