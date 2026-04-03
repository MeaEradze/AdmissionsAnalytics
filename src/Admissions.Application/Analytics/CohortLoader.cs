using Admissions.Application.Common;
using Admissions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Admissions.Application.Analytics;

public static class CohortLoader
{
    public static async Task<Dictionary<int, HealthResult>> HealthForYearAsync(
        IAppDbContext db,
        int year,
        AnalyticsOptions opt,
        CancellationToken ct,
        HealthCache? cache = null)
    {
        if (cache is not null && cache.TryGet(year, out var cached))
        {
            return cached!;
        }

        var cohort = await db.ProgramYearStats
            .AsNoTracking()
            .Where(s => s.Year == year)
            .ToListAsync(ct);

        var prevFp = await PreviousYearFirstPriorityAsync(db, year, ct);

        var result = HealthCalculator.Compute(cohort, prevFp, opt);
        cache?.Set(year, result);
        return result;
    }

    public static async Task<Dictionary<int, HealthResult>> HealthFromLoadedAsync(
        IAppDbContext db,
        int year,
        IReadOnlyList<ProgramYearStat> fullYearCohort,
        AnalyticsOptions opt,
        CancellationToken ct,
        HealthCache? cache = null)
    {
        if (cache is not null && cache.TryGet(year, out var cached))
        {
            return cached!;
        }

        var prevFp = await PreviousYearFirstPriorityAsync(db, year, ct);

        var result = HealthCalculator.Compute(fullYearCohort, prevFp, opt);
        cache?.Set(year, result);
        return result;
    }

    public static async Task<Dictionary<int, int>> PreviousYearFirstPriorityAsync(
        IAppDbContext db,
        int year,
        CancellationToken ct)
    {
        return await db.ProgramYearStats
            .AsNoTracking()
            .Where(s => s.Year == year - 1)
            .ToDictionaryAsync(s => s.ProgramId, s => s.FirstPriorityCount, ct);
    }

    public static ProgramYearStat? StatForYearOrLatest(IEnumerable<ProgramYearStat> stats, int year)
    {
        var list = stats.OrderBy(s => s.Year).ToList();
        return list.FirstOrDefault(s => s.Year == year) ?? list.LastOrDefault();
    }

    public static TrendSnap ToSnap(ProgramYearStat s) =>
        new(s.Year, s.AnnouncedPlaces, s.EnrolledCount, s.FirstPriorityCount, (double)s.AnnualFee);
}
