using Admissions.Application.Analytics;
using Admissions.Application.Common;
using Admissions.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Admissions.Application.Features.Compare;

public sealed record CompareProgramsQuery(string Ids, int Year) : IRequest<List<ProgramComparisonItemDto>>
{

    public const int MaxIds = 5;

    public static List<int> ParseIds(string? ids) =>
        (ids ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out int id) ? id : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
}

public sealed class CompareProgramsHandler
    : IRequestHandler<CompareProgramsQuery, List<ProgramComparisonItemDto>>
{
    private readonly IAppDbContext _db;
    private readonly AnalyticsOptions _opt;
    private readonly HealthCache _cache;

    public CompareProgramsHandler(IAppDbContext db, IOptions<AnalyticsOptions> opt, HealthCache cache)
    {
        _db = db;
        _opt = opt.Value;
        _cache = cache;
    }

    public async Task<List<ProgramComparisonItemDto>> Handle(CompareProgramsQuery q, CancellationToken ct)
    {
        var ids = CompareProgramsQuery.ParseIds(q.Ids);

        if (ids.Count == 0)
        {
            return [];
        }

        var programs = await _db.Programs
            .AsNoTracking()
            .Include(p => p.YearStats)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(ct);

        var byId = programs.ToDictionary(p => p.Id);
        var healthByYear = new Dictionary<int, Dictionary<int, HealthResult>>();

        var result = new List<ProgramComparisonItemDto>();
        foreach (int id in ids)
        {

            if (!byId.TryGetValue(id, out var program))
            {
                continue;
            }

            var stat = CohortLoader.StatForYearOrLatest(program.YearStats, q.Year);
            if (stat is null)
            {
                continue;
            }

            if (!healthByYear.TryGetValue(stat.Year, out var health))
            {
                health = await CohortLoader.HealthForYearAsync(_db, stat.Year, _opt, ct, _cache);
                healthByYear[stat.Year] = health;
            }

            var h = health[program.Id];
            var snaps = program.YearStats.OrderBy(s => s.Year).Select(CohortLoader.ToSnap).ToList();
            var conversion = ConversionBuilder.Build(program.Id, program.Name, snaps);
            var forecast = ForecastBuilder.Build(
                program.Id,
                program.Name,
                program.YearStats.OrderBy(s => s.Year).Select(s => (s.Year, s.EnrolledCount)).ToList(),
                _opt);

            result.Add(new ProgramComparisonItemDto
            {
                ProgramId = program.Id,
                ProgramName = program.Name,

                Year = stat.Year,
                IsFallback = stat.Year != q.Year,
                DemandScore = h.DemandScore,
                FillRateScore = h.FillRateScore,
                PriorityQualityScore = h.PriorityQualityScore,
                PriceScore = h.PriceScore,
                CompositeScore = h.CompositeScore,
                Category = h.Category,
                HistoricalAvgConversion = conversion.HistoricalAvgConversion,
                ForecastPointEstimate = forecast.PointEstimate,
            });
        }

        return result;
    }
}
