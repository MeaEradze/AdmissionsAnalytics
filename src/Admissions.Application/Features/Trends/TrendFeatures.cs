using Admissions.Application.Analytics;
using Admissions.Application.Common;
using Admissions.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Admissions.Application.Features.Trends;

public sealed record GetProgramTrendQuery(int ProgramId) : IRequest<TrendResultDto>;

public sealed class GetProgramTrendHandler : IRequestHandler<GetProgramTrendQuery, TrendResultDto>
{
    private readonly IAppDbContext _db;
    private readonly AnalyticsOptions _opt;

    public GetProgramTrendHandler(IAppDbContext db, IOptions<AnalyticsOptions> opt)
    {
        _db = db;
        _opt = opt.Value;
    }

    public async Task<TrendResultDto> Handle(GetProgramTrendQuery q, CancellationToken ct)
    {
        var program = await _db.Programs
            .AsNoTracking()
            .Include(p => p.YearStats)
            .FirstOrDefaultAsync(p => p.Id == q.ProgramId, ct)
            ?? throw new NotFoundException("პროგრამა", q.ProgramId);

        var snaps = program.YearStats
            .OrderBy(s => s.Year)
            .Select(CohortLoader.ToSnap)
            .ToList();

        return TrendBuilder.Build(program.Id, program.Name, snaps, _opt);
    }
}

public sealed record GetFieldTrendQuery(int FieldId) : IRequest<TrendResultDto>;

public sealed class GetFieldTrendHandler : IRequestHandler<GetFieldTrendQuery, TrendResultDto>
{
    private readonly IAppDbContext _db;
    private readonly AnalyticsOptions _opt;

    public GetFieldTrendHandler(IAppDbContext db, IOptions<AnalyticsOptions> opt)
    {
        _db = db;
        _opt = opt.Value;
    }

    public async Task<TrendResultDto> Handle(GetFieldTrendQuery q, CancellationToken ct)
    {
        var field = await _db.Fields.AsNoTracking().FirstOrDefaultAsync(f => f.Id == q.FieldId, ct)
            ?? throw new NotFoundException("დარგი", q.FieldId);

        var snaps = await AggregateSnapsAsync(
            _db.ProgramYearStats.AsNoTracking().Where(s => s.Program.FieldId == q.FieldId), ct);

        return TrendBuilder.Build(field.Id, field.Name, snaps, _opt);
    }

    internal static async Task<List<TrendSnap>> AggregateSnapsAsync(
        IQueryable<Domain.Entities.ProgramYearStat> source,
        CancellationToken ct)
    {
        var grouped = await source
            .GroupBy(s => s.Year)
            .Select(g => new
            {
                Year = g.Key,
                Announced = g.Sum(s => s.AnnouncedPlaces),
                Enrolled = g.Sum(s => s.EnrolledCount),
                FirstPriority = g.Sum(s => s.FirstPriorityCount),
                Fee = g.Average(s => (double)s.AnnualFee),
            })
            .OrderBy(x => x.Year)
            .ToListAsync(ct);

        return grouped
            .Select(x => new TrendSnap(x.Year, x.Announced, x.Enrolled, x.FirstPriority, x.Fee))
            .ToList();
    }
}

public sealed record GetUniversityTrendQuery(int UniversityId) : IRequest<TrendResultDto>;

public sealed class GetUniversityTrendHandler : IRequestHandler<GetUniversityTrendQuery, TrendResultDto>
{
    private readonly IAppDbContext _db;
    private readonly AnalyticsOptions _opt;

    public GetUniversityTrendHandler(IAppDbContext db, IOptions<AnalyticsOptions> opt)
    {
        _db = db;
        _opt = opt.Value;
    }

    public async Task<TrendResultDto> Handle(GetUniversityTrendQuery q, CancellationToken ct)
    {
        var uni = await _db.Universities.AsNoTracking().FirstOrDefaultAsync(u => u.Id == q.UniversityId, ct)
            ?? throw new NotFoundException("უნივერსიტეტი", q.UniversityId);

        var snaps = await GetFieldTrendHandler.AggregateSnapsAsync(
            _db.ProgramYearStats.AsNoTracking().Where(s => s.Program.UniversityId == q.UniversityId), ct);

        return TrendBuilder.Build(uni.Id, uni.Name, snaps, _opt);
    }
}
