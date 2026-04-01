namespace Admissions.Application.Analytics;

public static class Calc
{
    public static double Round(double value, int digits) =>
        Math.Round(value, digits, MidpointRounding.AwayFromZero);

    public static int RoundToInt(double value) =>
        (int)Math.Round(value, MidpointRounding.AwayFromZero);

    public static double MinMaxScore(double value, double min, double max, double neutral) =>
        max > min ? (value - min) / (max - min) * 100 : neutral;

    public static double PercentileRank(IReadOnlyList<double> sortedAscending, double value)
    {
        int n = sortedAscending.Count;
        if (n == 0)
        {
            return 50;
        }

        int below = LowerBound(sortedAscending, value);
        int upper = UpperBound(sortedAscending, value);
        int equal = upper - below;
        return (below + 0.5 * equal) / n * 100;
    }

    private static int LowerBound(IReadOnlyList<double> sorted, double value)
    {
        int lo = 0, hi = sorted.Count;
        while (lo < hi)
        {
            int mid = (lo + hi) / 2;
            if (sorted[mid] < value)
            {
                lo = mid + 1;
            }
            else
            {
                hi = mid;
            }
        }

        return lo;
    }

    private static int UpperBound(IReadOnlyList<double> sorted, double value)
    {
        int lo = 0, hi = sorted.Count;
        while (lo < hi)
        {
            int mid = (lo + hi) / 2;
            if (sorted[mid] <= value)
            {
                lo = mid + 1;
            }
            else
            {
                hi = mid;
            }
        }

        return lo;
    }

    public static double? Pearson(IReadOnlyList<double> xs, IReadOnlyList<double> ys)
    {
        int n = xs.Count;
        if (n < 2 || n != ys.Count)
        {
            return null;
        }

        double meanX = xs.Average();
        double meanY = ys.Average();
        double num = 0, denX = 0, denY = 0;
        for (int i = 0; i < n; i++)
        {
            double dx = xs[i] - meanX;
            double dy = ys[i] - meanY;
            num += dx * dy;
            denX += dx * dx;
            denY += dy * dy;
        }

        denX = Math.Sqrt(denX);
        denY = Math.Sqrt(denY);
        return denX > 0 && denY > 0 ? num / (denX * denY) : null;
    }

    public static (double Slope, double Intercept) Ols(IReadOnlyList<double> xs, IReadOnlyList<double> ys)
    {
        int n = xs.Count;
        double sumX = xs.Sum();
        double sumY = ys.Sum();
        double sumXy = 0, sumX2 = 0;
        for (int i = 0; i < n; i++)
        {
            sumXy += xs[i] * ys[i];
            sumX2 += xs[i] * xs[i];
        }

        double slope = (n * sumXy - sumX * sumY) / (n * sumX2 - sumX * sumX);
        double intercept = (sumY - slope * sumX) / n;
        return (slope, intercept);
    }

    public static int UpperMedian(IEnumerable<int> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        return sorted.Count == 0 ? 0 : sorted[sorted.Count / 2];
    }
}
