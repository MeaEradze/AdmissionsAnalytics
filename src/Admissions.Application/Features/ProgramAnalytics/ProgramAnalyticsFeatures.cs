using Admissions.Application.Analytics;
using Admissions.Application.Common;
using Admissions.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Admissions.Application.Features.ProgramAnalytics;

public sealed record GetProgramForecastQuery(int ProgramId) : IRequest<ProgramForecastDto>;

public sealed class GetProgramForecastHandler : IRequestHandler<GetProgramForecastQuery, ProgramForecastDto>
{
    private readonly IAppDbContext _db;
    private readonly AnalyticsOptions _opt;

    public GetProgramForecastHandler(IAppDbContext db, IOptions<AnalyticsOptions> opt)
    {
        _db = db;
        _opt = opt.Value;
    }

    public async Task<ProgramForecastDto> Handle(GetProgramForecastQuery q, CancellationToken ct)
    {
        var program = await _db.Programs
            .AsNoTracking()
            .Include(p => p.YearStats)
            .FirstOrDefaultAsync(p => p.Id == q.ProgramId, ct)
            ?? throw new NotFoundException("პროგრამა", q.ProgramId);

        var history = program.YearStats
            .OrderBy(s => s.Year)
            .Select(s => (s.Year, s.EnrolledCount))
            .ToList();

        return ForecastBuilder.Build(program.Id, program.Name, history, _opt);
    }
}

public sealed record GetProgramConversionQuery(int ProgramId) : IRequest<ProgramConversionDto>;

public sealed class GetProgramConversionHandler : IRequestHandler<GetProgramConversionQuery, ProgramConversionDto>
{
    private readonly IAppDbContext _db;

    public GetProgramConversionHandler(IAppDbContext db) => _db = db;

    public async Task<ProgramConversionDto> Handle(GetProgramConversionQuery q, CancellationToken ct)
    {
        var program = await _db.Programs
            .AsNoTracking()
            .Include(p => p.YearStats)
            .FirstOrDefaultAsync(p => p.Id == q.ProgramId, ct)
            ?? throw new NotFoundException("პროგრამა", q.ProgramId);

        var snaps = program.YearStats.OrderBy(s => s.Year).Select(CohortLoader.ToSnap).ToList();
        return ConversionBuilder.Build(program.Id, program.Name, snaps);
    }
}

public sealed record GetFeeSensitivityQuery(int ProgramId) : IRequest<FeeSensitivityDto>;

public sealed class GetFeeSensitivityHandler : IRequestHandler<GetFeeSensitivityQuery, FeeSensitivityDto>
{
    private readonly IAppDbContext _db;
    private readonly AnalyticsOptions _opt;

    public GetFeeSensitivityHandler(IAppDbContext db, IOptions<AnalyticsOptions> opt)
    {
        _db = db;
        _opt = opt.Value;
    }

    public async Task<FeeSensitivityDto> Handle(GetFeeSensitivityQuery q, CancellationToken ct)
    {
        var program = await _db.Programs
            .AsNoTracking()
            .Include(p => p.YearStats)
            .FirstOrDefaultAsync(p => p.Id == q.ProgramId, ct)
            ?? throw new NotFoundException("პროგრამა", q.ProgramId);

        var snaps = program.YearStats.OrderBy(s => s.Year).Select(CohortLoader.ToSnap).ToList();
        return FeeSensitivityCalculator.Build(program.Id, program.Name, snaps, _opt);
    }
}

public sealed record GetProgramBenchmarkQuery(int ProgramId, int Year) : IRequest<ProgramBenchmarkDto>;

public sealed class GetProgramBenchmarkHandler : IRequestHandler<GetProgramBenchmarkQuery, ProgramBenchmarkDto>
{
    private readonly IAppDbContext _db;
    private readonly AnalyticsOptions _opt;
    private readonly HealthCache _cache;

    public GetProgramBenchmarkHandler(IAppDbContext db, IOptions<AnalyticsOptions> opt, HealthCache cache)
    {
        _db = db;
        _opt = opt.Value;
        _cache = cache;
    }

    public async Task<ProgramBenchmarkDto> Handle(GetProgramBenchmarkQuery q, CancellationToken ct)
    {
        var program = await _db.Programs
            .AsNoTracking()
            .Include(p => p.YearStats)
            .FirstOrDefaultAsync(p => p.Id == q.ProgramId, ct)
            ?? throw new NotFoundException("პროგრამა", q.ProgramId);

        var myStat = CohortLoader.StatForYearOrLatest(program.YearStats, q.Year)
            ?? throw new NotFoundException("პროგრამის მონაცემები", q.ProgramId);
        int usedYear = myStat.Year;

        List<Domain.Entities.ProgramYearStat> peerStats;
        if (program.FieldId.HasValue)
        {
            peerStats = await _db.ProgramYearStats
                .AsNoTracking()
                .Where(s => s.Year == usedYear && s.Program.FieldId == program.FieldId.Value)
                .ToListAsync(ct);
        }
        else
        {
            peerStats = [myStat];
        }

        var health = await CohortLoader.HealthForYearAsync(_db, usedYear, _opt, ct, _cache);
        double myComposite = health[program.Id].CompositeScore;

        int n = peerStats.Count;
        double myFill = myStat.AnnouncedPlaces > 0 ? (double)myStat.EnrolledCount / myStat.AnnouncedPlaces : 0;

        static double Fill(Domain.Entities.ProgramYearStat s) =>
            s.AnnouncedPlaces > 0 ? (double)s.EnrolledCount / s.AnnouncedPlaces : 0;

        int medianFp = Calc.UpperMedian(peerStats.Select(s => s.FirstPriorityCount));

        int fillRank = 1 + peerStats.Count(s => Fill(s) > myFill);
        int feeRank = 1 + peerStats.Count(s => s.AnnualFee < myStat.AnnualFee);
        int demandRank0 = peerStats.Count(s => s.FirstPriorityCount > myStat.FirstPriorityCount);
        int healthRank0 = peerStats.Count(s =>
            health.TryGetValue(s.ProgramId, out var h) && h.CompositeScore > myComposite);

        double avgPeerComposite = peerStats
            .Select(s => health.TryGetValue(s.ProgramId, out var h) ? h.CompositeScore : 0)
            .DefaultIfEmpty(myComposite)
            .Average();

        double Pct(int rank0) => Calc.Round((double)(n - rank0) / n * 100, 1);

        return new ProgramBenchmarkDto
        {
            ProgramId = program.Id,
            ProgramName = program.Name,

            Year = usedYear,
            IsFallback = usedYear != q.Year,
            DemandRatioVsMedian = medianFp > 0
                ? Calc.Round((double)myStat.FirstPriorityCount / medianFp, 3)
                : 1,
            FillRateRankInField = fillRank,
            FeeRankInField = feeRank,
            HealthDeltaVsFieldAvg = Calc.Round(myComposite - avgPeerComposite, 2),
            DemandPercentile = Pct(demandRank0),
            FillRatePercentile = Pct(fillRank - 1),
            FeePercentile = Pct(feeRank - 1),
            HealthPercentile = Pct(healthRank0),
        };
    }
}

public sealed record GetPriorityDistributionQuery(int ProgramId, int Year) : IRequest<PriorityDistributionDto>;

public sealed class GetPriorityDistributionHandler
    : IRequestHandler<GetPriorityDistributionQuery, PriorityDistributionDto>
{
    private readonly IAppDbContext _db;

    public GetPriorityDistributionHandler(IAppDbContext db) => _db = db;

    public async Task<PriorityDistributionDto> Handle(GetPriorityDistributionQuery q, CancellationToken ct)
    {
        var program = await _db.Programs
            .AsNoTracking()
            .Include(p => p.YearStats)
            .ThenInclude(s => s.PriorityBreakdowns)
            .FirstOrDefaultAsync(p => p.Id == q.ProgramId, ct)
            ?? throw new NotFoundException("პროგრამა", q.ProgramId);

        var stat = CohortLoader.StatForYearOrLatest(program.YearStats, q.Year)
            ?? throw new NotFoundException("პროგრამის მონაცემები", q.ProgramId);

        var rows = stat.PriorityBreakdowns.OrderBy(b => b.Priority).ToList();

        if (rows.Count > 0)
        {
            double weighted = rows.Sum(r => (double)r.Count / r.Priority);
            return new PriorityDistributionDto
            {
                ProgramId = program.Id,
                ProgramName = program.Name,
                Year = stat.Year,
                FirstPriorityCount = stat.FirstPriorityCount,
                TotalPriorityCount = stat.TotalPriorityCount,
                WeightedDemandScore = Calc.Round(weighted, 2),
                InterestBreadth = rows.Count(r => r.Count > 0),
                Distribution = rows
                    .Select(r => new PriorityPointDto { Priority = r.Priority, Count = r.Count })
                    .ToList(),
                IsGranular = true,
            };
        }

        return new PriorityDistributionDto
        {
            ProgramId = program.Id,
            ProgramName = program.Name,
            Year = stat.Year,
            FirstPriorityCount = stat.FirstPriorityCount,
            TotalPriorityCount = stat.TotalPriorityCount,
            WeightedDemandScore = Calc.Round(stat.FirstPriorityCount, 2),
            InterestBreadth = stat.FirstPriorityCount > 0 ? 1 : 0,
            Distribution = null,
            IsGranular = false,
        };
    }
}
