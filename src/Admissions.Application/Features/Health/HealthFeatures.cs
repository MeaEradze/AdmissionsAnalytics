using Admissions.Application.Analytics;
using Admissions.Application.Common;
using Admissions.Application.Dtos;
using Admissions.Application.Features.Programs;
using Admissions.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Admissions.Application.Features.Health;

public sealed record GetProgramHealthQuery(int ProgramId, int Year) : IRequest<ProgramHealthDto>;

public sealed class GetProgramHealthHandler : IRequestHandler<GetProgramHealthQuery, ProgramHealthDto>
{
    private readonly IAppDbContext _db;
    private readonly AnalyticsOptions _opt;
    private readonly HealthCache _cache;

    public GetProgramHealthHandler(IAppDbContext db, IOptions<AnalyticsOptions> opt, HealthCache cache)
    {
        _db = db;
        _opt = opt.Value;
        _cache = cache;
    }

    public async Task<ProgramHealthDto> Handle(GetProgramHealthQuery q, CancellationToken ct)
    {
        var program = await _db.Programs
            .AsNoTracking()
            .Include(p => p.University)
            .Include(p => p.Field)
            .Include(p => p.YearStats)
            .FirstOrDefaultAsync(p => p.Id == q.ProgramId, ct)
            ?? throw new NotFoundException("პროგრამა", q.ProgramId);

        var stat = CohortLoader.StatForYearOrLatest(program.YearStats, q.Year)
            ?? throw new NotFoundException("პროგრამის მონაცემები", q.ProgramId);

        var health = await CohortLoader.HealthForYearAsync(_db, stat.Year, _opt, ct, _cache);
        var h = health[program.Id];

        return new ProgramHealthDto
        {
            ProgramId = program.Id,
            ProgramName = program.Name,
            UniversityName = program.University.Name,
            FieldName = program.Field?.Name ?? "",

            Year = stat.Year,
            IsFallback = stat.Year != q.Year,
            DemandScore = h.DemandScore,
            FillRateScore = h.FillRateScore,
            PriorityQualityScore = h.PriorityQualityScore,
            PriceScore = h.PriceScore,
            CompositeScore = h.CompositeScore,
            Category = h.Category,
            FillRate = h.FillRate,
            FirstPriorityCount = stat.FirstPriorityCount,
            EnrolledCount = stat.EnrolledCount,
            AnnouncedPlaces = stat.AnnouncedPlaces,
            AnnualFee = stat.AnnualFee,
        };
    }
}

public sealed record GetHealthListQuery(
    HealthCategory? Category,
    int? FieldId,
    int? UniversityId,
    int? Year,
    decimal? MinFee = null,
    decimal? MaxFee = null,
    int Page = 1,
    int PageSize = 20) : IRequest<HealthListResponse>;

public sealed class GetHealthListHandler : IRequestHandler<GetHealthListQuery, HealthListResponse>
{
    private readonly IAppDbContext _db;
    private readonly AnalyticsOptions _opt;
    private readonly HealthCache _cache;

    public GetHealthListHandler(IAppDbContext db, IOptions<AnalyticsOptions> opt, HealthCache cache)
    {
        _db = db;
        _opt = opt.Value;
        _cache = cache;
    }

    public async Task<HealthListResponse> Handle(GetHealthListQuery q, CancellationToken ct)
    {
        int page = q.Page < 1 ? 1 : q.Page;
        int pageSize = Math.Clamp(q.PageSize < 1 ? 20 : q.PageSize, 1, GetProgramsQuery.MaxPageSize);

        int? year = q.Year;
        if (year is null)
        {
            var latest = await _db.ProgramYearStats.AsNoTracking()
                .OrderByDescending(s => s.Year)
                .Select(s => (int?)s.Year)
                .FirstOrDefaultAsync(ct);
            year = latest;
        }

        if (year is null)
        {
            return new HealthListResponse { Data = [], Total = 0, Page = page, PageSize = pageSize };
        }

        var stats = await _db.ProgramYearStats
            .AsNoTracking()
            .Include(s => s.Program).ThenInclude(p => p.University)
            .Include(s => s.Program).ThenInclude(p => p.Field)
            .Where(s => s.Year == year.Value)
            .ToListAsync(ct);

        var health = await CohortLoader.HealthFromLoadedAsync(_db, year.Value, stats, _opt, ct, _cache);

        IEnumerable<Domain.Entities.ProgramYearStat> filtered = stats;
        if (q.FieldId.HasValue)
        {
            filtered = filtered.Where(s => s.Program.FieldId == q.FieldId.Value);
        }

        if (q.UniversityId.HasValue)
        {
            filtered = filtered.Where(s => s.Program.UniversityId == q.UniversityId.Value);
        }

        if (q.Category.HasValue)
        {
            filtered = filtered.Where(s => health[s.ProgramId].Category == q.Category.Value);
        }

        if (q.MinFee.HasValue)
        {
            filtered = filtered.Where(s => s.AnnualFee >= q.MinFee.Value);
        }

        if (q.MaxFee.HasValue)
        {
            filtered = filtered.Where(s => s.AnnualFee <= q.MaxFee.Value);
        }

        var ordered = filtered.OrderBy(s => s.ProgramId).ToList();

        var summary = new HealthListSummaryDto { Total = ordered.Count };
        double scoreSum = 0;
        foreach (var s in ordered)
        {
            var h = health[s.ProgramId];
            scoreSum += h.CompositeScore;
            switch (h.Category)
            {
                case HealthCategory.Growing: summary.GrowingCount++; break;
                case HealthCategory.Stable: summary.StableCount++; break;
                case HealthCategory.Risky: summary.RiskyCount++; break;
            }
        }

        summary.AverageScore = ordered.Count > 0 ? Calc.Round(scoreSum / ordered.Count, 1) : 0;

        var items = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s =>
            {
                var h = health[s.ProgramId];
                return new ProgramHealthDto
                {
                    ProgramId = s.ProgramId,
                    ProgramName = s.Program.Name,
                    UniversityName = s.Program.University.Name,
                    FieldName = s.Program.Field?.Name ?? "",
                    Year = year.Value,
                    DemandScore = h.DemandScore,
                    FillRateScore = h.FillRateScore,
                    PriorityQualityScore = h.PriorityQualityScore,
                    PriceScore = h.PriceScore,
                    CompositeScore = h.CompositeScore,
                    Category = h.Category,
                    FillRate = h.FillRate,
                    FirstPriorityCount = s.FirstPriorityCount,
                    EnrolledCount = s.EnrolledCount,
                    AnnouncedPlaces = s.AnnouncedPlaces,
                    AnnualFee = s.AnnualFee,
                };
            })
            .ToList();

        return new HealthListResponse
        {
            Data = items,
            Total = ordered.Count,
            Page = page,
            PageSize = pageSize,
            Summary = summary,
        };
    }
}
