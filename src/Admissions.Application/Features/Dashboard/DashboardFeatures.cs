using Admissions.Application.Analytics;
using Admissions.Application.Common;
using Admissions.Application.Dtos;
using Admissions.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Admissions.Application.Features.Dashboard;

public sealed record GetDashboardSummaryQuery(int Year) : IRequest<DashboardSummaryDto>;

public sealed class GetDashboardSummaryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IAppDbContext _db;
    private readonly AnalyticsOptions _opt;
    private readonly HealthCache _cache;

    public GetDashboardSummaryHandler(IAppDbContext db, IOptions<AnalyticsOptions> opt, HealthCache cache)
    {
        _db = db;
        _opt = opt.Value;
        _cache = cache;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery q, CancellationToken ct)
    {
        var stats = await _db.ProgramYearStats
            .AsNoTracking()
            .Include(s => s.Program).ThenInclude(p => p.University)
            .Include(s => s.Program).ThenInclude(p => p.Field)
            .Where(s => s.Year == q.Year)
            .ToListAsync(ct);

        var health = await CohortLoader.HealthFromLoadedAsync(_db, q.Year, stats, _opt, ct, _cache);

        var fills = stats
            .Where(s => s.AnnouncedPlaces > 0)
            .Select(s => (double)s.EnrolledCount / s.AnnouncedPlaces)
            .ToList();

        var growing = stats
            .Where(s => health[s.ProgramId].Category == HealthCategory.Growing)
            .OrderByDescending(s => health[s.ProgramId].CompositeScore)
            .Take(5)
            .Select(s => new TopProgramDto
            {
                ProgramId = s.ProgramId,
                ProgramName = s.Program.Name,
                UniversityName = s.Program.University.Name,
                HealthScore = health[s.ProgramId].CompositeScore,
            })
            .ToList();

        var risky = stats
            .Where(s => health[s.ProgramId].Category == HealthCategory.Risky)
            .OrderBy(s => health[s.ProgramId].CompositeScore)
            .Take(5)
            .Select(s => new TopProgramDto
            {
                ProgramId = s.ProgramId,
                ProgramName = s.Program.Name,
                UniversityName = s.Program.University.Name,
                HealthScore = health[s.ProgramId].CompositeScore,
            })
            .ToList();

        var topFields = stats
            .Where(s => s.Program.FieldId != null)
            .GroupBy(s => new { Id = s.Program.FieldId!.Value, s.Program.Field!.Name })
            .Select(g =>
            {
                var groupFills = g
                    .Where(s => s.AnnouncedPlaces > 0)
                    .Select(s => (double)s.EnrolledCount / s.AnnouncedPlaces)
                    .ToList();
                return new TopFieldDto
                {
                    FieldId = g.Key.Id,
                    FieldName = g.Key.Name,
                    Demand = g.Sum(s => s.FirstPriorityCount),
                    FillRate = groupFills.Count > 0 ? groupFills.Average() : 0,
                };
            })
            .OrderByDescending(f => f.Demand)
            .Take(5)
            .ToList();

        return new DashboardSummaryDto
        {
            Year = q.Year,
            TotalPrograms = stats.Count,
            TotalUniversities = await _db.Universities.CountAsync(ct),
            TotalFields = await _db.Fields.CountAsync(ct),
            AvgFillRate = fills.Count > 0 ? Calc.Round(fills.Average(), 4) : 0,
            TotalDemand = stats.Sum(s => s.FirstPriorityCount),
            TopGrowingPrograms = growing,
            TopRiskyPrograms = risky,
            TopFields = topFields,
        };
    }
}
