using Admissions.Application.Analytics;

namespace Admissions.Tests.Unit;

public class ConversionBuilderTests
{
    [Fact]
    public void Build_RatesAndAverage_HandComputed_MockProgram1()
    {

        var result = ConversionBuilder.Build(1, "ქართული ფილოლოგია",
        [
            new TrendSnap(2023, 120, 98, 145, 2250),
            new TrendSnap(2024, 120, 108, 163, 2250),
            new TrendSnap(2025, 130, 115, 178, 2500),
        ]);

        Assert.Equal(3, result.YoYDeltas.Count);
        Assert.Equal(0.6759, result.YoYDeltas[0].ConversionRate, 10);
        Assert.Equal(0.6626, result.YoYDeltas[1].ConversionRate, 10);
        Assert.Equal(0.6461, result.YoYDeltas[2].ConversionRate, 10);
        Assert.Equal(0.6615, result.HistoricalAvgConversion, 10);

        Assert.Null(result.YoYDeltas[0].Delta);
        double rate23 = 98.0 / 145, rate24 = 108.0 / 163, rate25 = 115.0 / 178;
        Assert.Equal((rate24 - rate23) / rate23, result.YoYDeltas[1].Delta!.Value, 10);
        Assert.Equal((rate25 - rate24) / rate24, result.YoYDeltas[2].Delta!.Value, 10);
    }

    [Fact]
    public void Build_ZeroFirstPriority_RateZeroDeltaNull()
    {
        var result = ConversionBuilder.Build(1, "x",
        [
            new TrendSnap(2023, 100, 50, 0, 2000),
            new TrendSnap(2024, 100, 60, 80, 2000),
        ]);

        Assert.Equal(0, result.YoYDeltas[0].ConversionRate);
        Assert.Null(result.YoYDeltas[1].Delta);
    }

    [Fact]
    public void Build_Empty_ZeroAverage()
    {
        var result = ConversionBuilder.Build(1, "x", []);
        Assert.Empty(result.YoYDeltas);
        Assert.Equal(0, result.HistoricalAvgConversion);
    }
}
