using Admissions.Application.Analytics;
using Admissions.Application.Common;
using Admissions.Application.Dtos;
using Admissions.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Admissions.Application.Features.Market;

internal sealed record GapSourceRow(int FieldId, int FirstPriorityCount, int AnnouncedPlaces, int EnrolledCount);

internal static class FieldGapBuilder
{

    public static FieldGapDto Build(
        int fieldId,
        string fieldName,
        IReadOnlyList<GapSourceRow> fieldRows,
        AnalyticsOptions opt)
    {
        int aggregateDemand = fieldRows.Sum(r => r.FirstPriorityCount);
        int totalSupply = fieldRows.Sum(r => r.AnnouncedPlaces);
        double ratio = totalSupply > 0 ? (double)aggregateDemand / totalSupply : 0;

        var fills = fieldRows
            .Where(r => r.AnnouncedPlaces > 0)
            .Select(r => (double)r.EnrolledCount / r.AnnouncedPlaces)
            .ToList();
        double avgFill = fills.Count > 0 ? fills.Average() : 0;

        var severity = ratio > opt.GapHighDemandSupplyRatio && avgFill > opt.GapHighMinAvgFillRate
            ? GapSeverity.High
            : ratio > opt.GapMediumDemandSupplyRatio
                ? GapSeverity.Medium
                : GapSeverity.Low;

        return new FieldGapDto
        {
            FieldId = fieldId,
            FieldName = fieldName,
            AggregateDemand = aggregateDemand,
            TotalSupply = totalSupply,
            DemandSupplyRatio = Calc.Round(ratio, 3),
            AvgFillRate = Calc.Round(avgFill, 4),
            GapSeverity = severity,
            ProgramCount = fieldRows.Count,
        };
    }

    public static async Task<List<FieldGapDto>> BuildAllAsync(
        IAppDbContext db,
        int year,
        AnalyticsOptions opt,
        CancellationToken ct)
    {
        var fields = await db.Fields.AsNoTracking().OrderBy(f => f.Id).ToListAsync(ct);

        var rows = await db.ProgramYearStats
            .AsNoTracking()
            .Where(s => s.Year == year && s.Program.FieldId != null)
            .Select(s => new GapSourceRow(
                s.Program.FieldId!.Value, s.FirstPriorityCount, s.AnnouncedPlaces, s.EnrolledCount))
            .ToListAsync(ct);

        var byField = rows.ToLookup(r => r.FieldId);

        return fields
            .Select(f => Build(f.Id, f.Name, byField[f.Id].ToList(), opt))
            .ToList();
    }
}

public sealed record GetMarketGapsQuery(int Year) : IRequest<List<FieldGapDto>>;

public sealed class GetMarketGapsHandler : IRequestHandler<GetMarketGapsQuery, List<FieldGapDto>>
{
    private readonly IAppDbContext _db;
    private readonly AnalyticsOptions _opt;

    public GetMarketGapsHandler(IAppDbContext db, IOptions<AnalyticsOptions> opt)
    {
        _db = db;
        _opt = opt.Value;
    }

    public async Task<List<FieldGapDto>> Handle(GetMarketGapsQuery q, CancellationToken ct)
    {
        var gaps = await FieldGapBuilder.BuildAllAsync(_db, q.Year, _opt, ct);
        return gaps.OrderBy(g => (int)g.GapSeverity).ToList();
    }
}

public sealed record GetMarketOverviewQuery(int Year) : IRequest<MarketOverviewDto>;

public sealed class GetMarketOverviewHandler : IRequestHandler<GetMarketOverviewQuery, MarketOverviewDto>
{
    private readonly IAppDbContext _db;
    private readonly AnalyticsOptions _opt;
    private readonly HealthCache _cache;

    public GetMarketOverviewHandler(IAppDbContext db, IOptions<AnalyticsOptions> opt, HealthCache cache)
    {
        _db = db;
        _opt = opt.Value;
        _cache = cache;
    }

    public async Task<MarketOverviewDto> Handle(GetMarketOverviewQuery q, CancellationToken ct)
    {
        var gaps = await FieldGapBuilder.BuildAllAsync(_db, q.Year, _opt, ct);

        var stats = await _db.ProgramYearStats
            .AsNoTracking()
            .Where(s => s.Year == q.Year)
            .ToListAsync(ct);

        var health = await CohortLoader.HealthFromLoadedAsync(_db, q.Year, stats, _opt, ct, _cache);

        var fills = stats
            .Where(s => s.AnnouncedPlaces > 0)
            .Select(s => (double)s.EnrolledCount / s.AnnouncedPlaces)
            .ToList();

        var riskiest = gaps.OrderBy(g => (int)g.GapSeverity).FirstOrDefault();

        return new MarketOverviewDto
        {
            Year = q.Year,
            TotalPrograms = stats.Count,
            TotalUniversities = await _db.Universities.CountAsync(ct),
            TotalFields = gaps.Count,
            TotalSupply = stats.Sum(s => s.AnnouncedPlaces),
            TotalEnrolled = stats.Sum(s => s.EnrolledCount),
            TotalDemand = stats.Sum(s => s.FirstPriorityCount),
            AvgFillRate = fills.Count > 0 ? Calc.Round(fills.Average(), 4) : 0,
            AvgHealthScore = health.Count > 0
                ? Calc.Round(health.Values.Average(h => h.CompositeScore), 1)
                : 0,
            TopFields = gaps.OrderByDescending(g => g.AggregateDemand).Take(3).ToList(),
            TopRiskyFieldByGap = riskiest is null
                ? null
                : new TopRiskyFieldRefDto
                {
                    FieldId = riskiest.FieldId,
                    FieldName = riskiest.FieldName,
                    GapSeverity = riskiest.GapSeverity.ToString(),
                },
        };
    }
}
