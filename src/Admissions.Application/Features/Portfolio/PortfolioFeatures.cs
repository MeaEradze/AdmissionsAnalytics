using Admissions.Application.Analytics;
using Admissions.Application.Common;
using Admissions.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Admissions.Application.Features.Portfolio;

public sealed record GetUniversityPortfolioQuery(int UniversityId, int Year) : IRequest<List<PortfolioItemDto>>;

public sealed class GetUniversityPortfolioHandler
    : IRequestHandler<GetUniversityPortfolioQuery, List<PortfolioItemDto>>
{
    private readonly IAppDbContext _db;
    private readonly AnalyticsOptions _opt;
    private readonly HealthCache _cache;

    public GetUniversityPortfolioHandler(IAppDbContext db, IOptions<AnalyticsOptions> opt, HealthCache cache)
    {
        _db = db;
        _opt = opt.Value;
        _cache = cache;
    }

    public async Task<List<PortfolioItemDto>> Handle(GetUniversityPortfolioQuery q, CancellationToken ct)
    {
        _ = await _db.Universities.AsNoTracking().FirstOrDefaultAsync(u => u.Id == q.UniversityId, ct)
            ?? throw new NotFoundException("უნივერსიტეტი", q.UniversityId);

        var uniStats = await _db.ProgramYearStats
            .AsNoTracking()
            .Include(s => s.Program).ThenInclude(p => p.Field)
            .Where(s => s.Year == q.Year && s.Program.UniversityId == q.UniversityId)
            .OrderBy(s => s.ProgramId)
            .ToListAsync(ct);

        var health = await CohortLoader.HealthForYearAsync(_db, q.Year, _opt, ct, _cache);

        var fieldTotals = await _db.ProgramYearStats
            .AsNoTracking()
            .Where(s => s.Year == q.Year && s.Program.FieldId != null)
            .GroupBy(s => s.Program.FieldId!.Value)
            .Select(g => new { FieldId = g.Key, Total = g.Sum(x => x.FirstPriorityCount) })
            .ToDictionaryAsync(x => x.FieldId, x => x.Total, ct);

        return uniStats.Select(s =>
        {
            var h = health[s.ProgramId];
            int? fieldId = s.Program.FieldId;
            int fieldTotal = fieldId.HasValue ? fieldTotals.GetValueOrDefault(fieldId.Value) : 0;

            return new PortfolioItemDto
            {
                ProgramId = s.ProgramId,
                ProgramName = s.Program.Name,
                FieldId = fieldId ?? 0,
                FieldName = s.Program.Field?.Name ?? "",
                CompositeScore = h.CompositeScore,
                Category = h.Category,
                MarketShareInField = fieldTotal > 0
                    ? Calc.Round((double)s.FirstPriorityCount / fieldTotal * 100, 2)
                    : 0,
            };
        }).ToList();
    }
}
