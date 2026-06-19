using Admissions.Application.Analytics;
using Admissions.Application.Common;
using Admissions.Application.Dtos;
using Admissions.Domain.Entities;
using Admissions.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Admissions.Application.Features.Programs;

public sealed record GetProgramsQuery(
    int? UniversityId,
    int? FieldId,
    int? Year,
    decimal? MinFee,
    decimal? MaxFee,
    string? Search,
    HealthCategory? HealthCategory,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResponse<ProgramListItemDto>>
{
    public const int MaxPageSize = 1000;
}

public sealed class GetProgramsHandler : IRequestHandler<GetProgramsQuery, PagedResponse<ProgramListItemDto>>
{
    private readonly IAppDbContext _db;
    private readonly AnalyticsOptions _opt;
    private readonly HealthCache _cache;

    public GetProgramsHandler(
        IAppDbContext db,
        Microsoft.Extensions.Options.IOptions<AnalyticsOptions> opt,
        HealthCache cache)
    {
        _db = db;
        _opt = opt.Value;
        _cache = cache;
    }

    public async Task<PagedResponse<ProgramListItemDto>> Handle(GetProgramsQuery q, CancellationToken ct)
    {

        List<ProgramYearStat> candidates;
        var healthByYear = new Dictionary<int, Dictionary<int, HealthResult>>();
        if (q.Year.HasValue)
        {
            candidates = await _db.ProgramYearStats
                .AsNoTracking()
                .Include(s => s.Program).ThenInclude(p => p.University)
                .Include(s => s.Program).ThenInclude(p => p.Field)
                .Where(s => s.Year == q.Year.Value)
                .ToListAsync(ct);

            healthByYear[q.Year.Value] =
                await CohortLoader.HealthFromLoadedAsync(_db, q.Year.Value, candidates, _opt, ct, _cache);
        }
        else
        {
            var all = await _db.ProgramYearStats
                .AsNoTracking()
                .Include(s => s.Program).ThenInclude(p => p.University)
                .Include(s => s.Program).ThenInclude(p => p.Field)
                .ToListAsync(ct);
            candidates = all
                .GroupBy(s => s.ProgramId)
                .Select(g => g.OrderByDescending(s => s.Year).First())
                .ToList();

            foreach (int year in candidates.Select(s => s.Year).Distinct())
            {
                var cohort = all.Where(s => s.Year == year).ToList();
                healthByYear[year] =
                    await CohortLoader.HealthFromLoadedAsync(_db, year, cohort, _opt, ct, _cache);
            }
        }

        IEnumerable<ProgramYearStat> filtered = candidates;
        if (q.UniversityId.HasValue)
        {
            filtered = filtered.Where(s => s.Program.UniversityId == q.UniversityId.Value);
        }

        if (q.FieldId.HasValue)
        {
            filtered = filtered.Where(s => s.Program.FieldId == q.FieldId.Value);
        }

        if (q.MinFee.HasValue)
        {
            filtered = filtered.Where(s => s.AnnualFee >= q.MinFee.Value);
        }

        if (q.MaxFee.HasValue)
        {
            filtered = filtered.Where(s => s.AnnualFee <= q.MaxFee.Value);
        }

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            string term = q.Search.Trim();
            filtered = filtered.Where(s =>
                s.Program.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                s.Program.University.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (q.HealthCategory.HasValue)
        {
            filtered = filtered.Where(s =>
                healthByYear[s.Year].TryGetValue(s.ProgramId, out var h) &&
                h.Category == q.HealthCategory.Value);
        }

        var ordered = filtered.OrderBy(s => s.ProgramId).ToList();

        int page = q.Page < 1 ? 1 : q.Page;
        int pageSize = Math.Clamp(q.PageSize < 1 ? 20 : q.PageSize, 1, GetProgramsQuery.MaxPageSize);

        var items = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s =>
            {
                var h = healthByYear[s.Year].GetValueOrDefault(s.ProgramId);
                return new ProgramListItemDto
                {
                    Id = s.ProgramId,
                    Name = s.Program.Name,
                    Code = s.Program.Code,
                    UniversityId = s.Program.UniversityId,
                    UniversityName = s.Program.University.Name,
                    FieldId = s.Program.FieldId ?? 0,
                    FieldName = s.Program.Field?.Name ?? "",
                    Year = s.Year,
                    AnnouncedPlaces = s.AnnouncedPlaces,
                    EnrolledCount = s.EnrolledCount,
                    FirstPriorityCount = s.FirstPriorityCount,
                    AnnualFee = s.AnnualFee,
                    CompositeScore = h?.CompositeScore,
                    Category = h?.Category,
                };
            })
            .ToList();

        return new PagedResponse<ProgramListItemDto>
        {
            Data = items,
            Total = ordered.Count,
            Page = page,
            PageSize = pageSize,
        };
    }
}

public sealed record GetProgramDetailQuery(int Id) : IRequest<ProgramDetailDto>;

public sealed class GetProgramDetailHandler : IRequestHandler<GetProgramDetailQuery, ProgramDetailDto>
{
    private readonly IAppDbContext _db;

    public GetProgramDetailHandler(IAppDbContext db) => _db = db;

    public async Task<ProgramDetailDto> Handle(GetProgramDetailQuery q, CancellationToken ct)
    {
        var program = await _db.Programs
            .AsNoTracking()
            .Include(p => p.University)
            .Include(p => p.Field)
            .Include(p => p.YearStats)
            .FirstOrDefaultAsync(p => p.Id == q.Id, ct)
            ?? throw new NotFoundException("პროგრამა", q.Id);

        return new ProgramDetailDto
        {
            Id = program.Id,
            Name = program.Name,
            Code = program.Code,
            DegreeLevel = program.DegreeLevel,
            University = new UniversityDto
            {
                Id = program.University.Id,
                Name = program.University.Name,
                ShortName = program.University.ShortName,
                Code = program.University.Code,
            },
            Field = program.Field is null
                ? new FieldDto { Id = 0, Name = "" }
                : new FieldDto { Id = program.Field.Id, Name = program.Field.Name, Code = program.Field.Code },
            YearStats = program.YearStats
                .OrderBy(s => s.Year)
                .Select(s => new ProgramYearDto
                {
                    Year = s.Year,
                    AnnouncedPlaces = s.AnnouncedPlaces,
                    EnrolledCount = s.EnrolledCount,
                    FirstPriorityCount = s.FirstPriorityCount,
                    TotalPriorityCount = s.TotalPriorityCount,
                    AnnualFee = s.AnnualFee,
                    GrantFullCount = s.GrantFullCount,
                    GrantPartialCount = s.GrantPartialCount,
                })
                .ToList(),
        };
    }
}

public sealed record UpdateProgramYearStatsCommand(int ProgramId, int Year, UpdateProgramYearRequest Body)
    : IRequest<Unit>;

public sealed class UpdateProgramYearStatsHandler : IRequestHandler<UpdateProgramYearStatsCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly HealthCache _cache;

    public UpdateProgramYearStatsHandler(IAppDbContext db, HealthCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<Unit> Handle(UpdateProgramYearStatsCommand cmd, CancellationToken ct)
    {
        var program = await _db.Programs
            .Include(p => p.YearStats)
            .FirstOrDefaultAsync(p => p.Id == cmd.ProgramId, ct)
            ?? throw new NotFoundException("პროგრამა", cmd.ProgramId);

        var stat = program.YearStats.FirstOrDefault(s => s.Year == cmd.Year);
        if (stat is null)
        {
            stat = new ProgramYearStat { Year = cmd.Year };
            program.YearStats.Add(stat);
        }

        stat.AnnouncedPlaces = cmd.Body.AnnouncedPlaces;
        stat.EnrolledCount = cmd.Body.EnrolledCount;
        stat.FirstPriorityCount = cmd.Body.FirstPriorityCount;
        stat.TotalPriorityCount = cmd.Body.TotalPriorityCount;
        stat.AnnualFee = cmd.Body.AnnualFee;
        stat.GrantFullCount = cmd.Body.GrantFullCount;
        stat.GrantPartialCount = cmd.Body.GrantPartialCount;

        await _db.SaveChangesAsync(ct);
        _cache.Clear();
        return Unit.Value;
    }
}

public sealed record AssignProgramFieldCommand(int ProgramId, int? FieldId) : IRequest<Unit>;

public sealed class AssignProgramFieldHandler : IRequestHandler<AssignProgramFieldCommand, Unit>
{
    private readonly IAppDbContext _db;

    public AssignProgramFieldHandler(IAppDbContext db) => _db = db;

    public async Task<Unit> Handle(AssignProgramFieldCommand cmd, CancellationToken ct)
    {
        var program = await _db.Programs.FirstOrDefaultAsync(p => p.Id == cmd.ProgramId, ct)
            ?? throw new NotFoundException("პროგრამა", cmd.ProgramId);

        if (cmd.FieldId.HasValue)
        {
            _ = await _db.Fields.FirstOrDefaultAsync(f => f.Id == cmd.FieldId.Value, ct)
                ?? throw new NotFoundException("დარგი", cmd.FieldId.Value);
        }

        program.FieldId = cmd.FieldId;
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
