using Admissions.Application.Common;
using Admissions.Application.Dtos;

namespace Admissions.Application.Analytics;

public static class FeeSensitivityCalculator
{
    public static FeeSensitivityDto Build(
        int programId,
        string programName,
        IReadOnlyList<TrendSnap> snaps,
        AnalyticsOptions opt)
    {
        var insufficient = new FeeSensitivityDto
        {
            ProgramId = programId,
            ProgramName = programName,
            Indicative = true,
            InsufficientData = true,
            PearsonCorrelation = null,
            SlopeSign = "flat",
        };

        if (snaps.Count < 2)
        {
            return insufficient;
        }

        var deltaFees = new List<double>();
        var deltaDemands = new List<double>();
        for (int i = 1; i < snaps.Count; i++)
        {
            var prev = snaps[i - 1];
            var cur = snaps[i];
            deltaFees.Add(prev.Fee > 0 ? (cur.Fee - prev.Fee) / prev.Fee : 0);
            deltaDemands.Add(prev.FirstPriority > 0
                ? (double)(cur.FirstPriority - prev.FirstPriority) / prev.FirstPriority
                : 0);
        }

        if (deltaFees.Count < 2)
        {
            return insufficient;
        }

        double? r = Calc.Pearson(deltaFees, deltaDemands);
        double? rounded = r is null ? null : Calc.Round(r.Value, 4);

        string slopeSign = rounded is null
            ? "flat"
            : rounded > opt.SlopeSignPositiveCutoff
                ? "positive"
                : rounded < opt.SlopeSignNegativeCutoff
                    ? "negative"
                    : "flat";

        return new FeeSensitivityDto
        {
            ProgramId = programId,
            ProgramName = programName,
            Indicative = true,
            InsufficientData = false,
            PearsonCorrelation = rounded,
            SlopeSign = slopeSign,
        };
    }
}
