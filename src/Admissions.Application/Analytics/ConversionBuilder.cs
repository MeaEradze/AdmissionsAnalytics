using Admissions.Application.Dtos;

namespace Admissions.Application.Analytics;

public static class ConversionBuilder
{
    public static ProgramConversionDto Build(
        int programId,
        string programName,
        IReadOnlyList<TrendSnap> snaps)
    {
        var years = new List<ConversionYearDto>();

        for (int i = 0; i < snaps.Count; i++)
        {
            var snap = snaps[i];
            double rate = snap.FirstPriority > 0
                ? (double)snap.Enrolled / snap.FirstPriority
                : 0;

            double? delta = null;
            if (i > 0)
            {
                var prev = snaps[i - 1];
                double? prevRate = prev.FirstPriority > 0
                    ? (double)prev.Enrolled / prev.FirstPriority
                    : null;
                if (prevRate is > 0)
                {
                    delta = (rate - prevRate.Value) / prevRate.Value;
                }
            }

            years.Add(new ConversionYearDto
            {
                Year = snap.Year,
                ConversionRate = Calc.Round(rate, 4),
                Delta = delta,
            });
        }

        double avg = years.Count > 0 ? years.Average(y => y.ConversionRate) : 0;

        return new ProgramConversionDto
        {
            ProgramId = programId,
            ProgramName = programName,
            HistoricalAvgConversion = Calc.Round(avg, 4),
            YoYDeltas = years,
        };
    }
}
