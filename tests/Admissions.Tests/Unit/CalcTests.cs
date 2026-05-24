using Admissions.Application.Analytics;

namespace Admissions.Tests.Unit;

public class CalcTests
{
    [Theory]
    [InlineData(0.5, 0, 1, 50)]
    [InlineData(1, 0, 1, 100)]
    [InlineData(0, 0, 1, 0)]
    public void MinMaxScore_NormalizesToHundredScale(double value, double min, double max, double expected)
    {
        Assert.Equal(expected, Calc.MinMaxScore(value, min, max, 50), 6);
    }

    [Fact]
    public void MinMaxScore_NoSpread_ReturnsNeutral()
    {
        Assert.Equal(50, Calc.MinMaxScore(7, 7, 7, 50));
    }

    [Fact]
    public void PercentileRank_MidRank_HandComputed()
    {
        double[] sorted = [10, 20, 30];
        Assert.Equal((0 + 0.5) / 3 * 100, Calc.PercentileRank(sorted, 10), 4);
        Assert.Equal(50, Calc.PercentileRank(sorted, 20), 4);
        Assert.Equal((2 + 0.5) / 3 * 100, Calc.PercentileRank(sorted, 30), 4);
    }

    [Fact]
    public void PercentileRank_Ties_UseMidRank()
    {
        double[] sorted = [10, 10, 20];
        Assert.Equal((0 + 0.5 * 2) / 3 * 100, Calc.PercentileRank(sorted, 10), 4);
        Assert.Equal((2 + 0.5) / 3 * 100, Calc.PercentileRank(sorted, 20), 4);
    }

    [Fact]
    public void PercentileRank_SingleValue_IsNeutralFifty()
    {
        Assert.Equal(50, Calc.PercentileRank([5.0], 5.0), 4);
    }

    [Fact]
    public void UpperMedian_MatchesMockIndexing()
    {

        Assert.Equal(3, Calc.UpperMedian([1, 2, 3, 4]));
        Assert.Equal(2, Calc.UpperMedian([3, 1, 2]));
        Assert.Equal(5, Calc.UpperMedian([5]));
        Assert.Equal(0, Calc.UpperMedian([]));
    }

    [Fact]
    public void Round_HalfAwayFromZero_LikeJs()
    {
        Assert.Equal(0.13, Calc.Round(0.125, 2));
        Assert.Equal(112, Calc.RoundToInt(111.6));
        Assert.Equal(2, Calc.RoundToInt(1.5));
    }

    [Fact]
    public void Pearson_PerfectlyAnticorrelated_IsMinusOne()
    {
        double? r = Calc.Pearson([0, 0.111111], [0.124138, 0.092025]);
        Assert.NotNull(r);
        Assert.Equal(-1, r.Value, 4);
    }

    [Fact]
    public void Pearson_ZeroVariance_ReturnsNull()
    {
        Assert.Null(Calc.Pearson([1, 1], [2, 3]));
    }

    [Fact]
    public void Ols_HandComputedSlopeIntercept()
    {

        var (slope, intercept) = Calc.Ols([2023, 2024, 2025], [98, 108, 115]);
        Assert.Equal(8.5, slope, 6);
        Assert.Equal(107 - 8.5 * 2024, intercept, 4);
    }
}
