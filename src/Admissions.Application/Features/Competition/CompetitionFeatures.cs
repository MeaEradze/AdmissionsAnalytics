using Admissions.Application.Analytics;
using Admissions.Application.Common;
using Admissions.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Admissions.Application.Features.Competition;

public sealed record GetFieldCompetitionQuery(int FieldId, int Year) : IRequest<FieldCompetitionDto>;

public sealed class GetFieldCompetitionHandler : IRequestHandler<GetFieldCompetitionQuery, FieldCompetitionDto>
{
    private readonly IAppDbContext _db;

    public GetFieldCompetitionHandler(IAppDbContext db) => _db = db;

    public async Task<FieldCompetitionDto> Handle(GetFieldCompetitionQuery q, CancellationToken ct)
    {
        var field = await _db.Fields.AsNoTracking().FirstOrDefaultAsync(f => f.Id == q.FieldId, ct)
            ?? throw new NotFoundException("დარგი", q.FieldId);

        var rows = await _db.ProgramYearStats
            .AsNoTracking()
            .Where(s => s.Year == q.Year && s.Program.FieldId == q.FieldId)
            .Select(s => new
            {
                s.FirstPriorityCount,
                s.Program.UniversityId,
                UniversityName = s.Program.University.Name,
            })
            .ToListAsync(ct);

        int totalDemand = rows.Sum(r => r.FirstPriorityCount);

        var universities = rows
            .GroupBy(r => new { r.UniversityId, r.UniversityName })
            .Select(g => new UniversityShareDto
            {
                UniversityId = g.Key.UniversityId,
                UniversityName = g.Key.UniversityName,
                FirstPriorityCount = g.Sum(r => r.FirstPriorityCount),
                MarketSharePct = totalDemand > 0
                    ? Calc.Round((double)g.Sum(r => r.FirstPriorityCount) / totalDemand * 100, 2)
                    : 0,
            })
            .OrderByDescending(u => u.FirstPriorityCount)
            .ToList();

        return new FieldCompetitionDto
        {
            FieldId = field.Id,
            FieldName = field.Name,
            Year = q.Year,
            TotalDemand = totalDemand,
            Universities = universities,
        };
    }
}

public sealed record GetProgramCompetitionQuery(int ProgramId, int FromYear, int ToYear)
    : IRequest<ProgramCompetitionTrendDto>;

public sealed class GetProgramCompetitionHandler
    : IRequestHandler<GetProgramCompetitionQuery, ProgramCompetitionTrendDto>
{
    private readonly IAppDbContext _db;

    public GetProgramCompetitionHandler(IAppDbContext db) => _db = db;

    public async Task<ProgramCompetitionTrendDto> Handle(GetProgramCompetitionQuery q, CancellationToken ct)
    {
        var program = await _db.Programs
            .AsNoTracking()
            .Include(p => p.Field)
            .FirstOrDefaultAsync(p => p.Id == q.ProgramId, ct)
            ?? throw new NotFoundException("პროგრამა", q.ProgramId);

        var stats = await _db.ProgramYearStats
            .AsNoTracking()
            .Where(s => s.ProgramId == q.ProgramId && s.Year >= q.FromYear && s.Year <= q.ToYear)
            .OrderBy(s => s.Year)
            .ToListAsync(ct);

        Dictionary<int, int> fieldTotals = [];
        if (program.FieldId.HasValue)
        {
            fieldTotals = await _db.ProgramYearStats
                .AsNoTracking()
                .Where(s => s.Program.FieldId == program.FieldId.Value &&
                            s.Year >= q.FromYear && s.Year <= q.ToYear)
                .GroupBy(s => s.Year)
                .Select(g => new { Year = g.Key, Total = g.Sum(x => x.FirstPriorityCount) })
                .ToDictionaryAsync(x => x.Year, x => x.Total, ct);
        }

        var years = stats.Select(s =>
        {
            int fieldTotal = fieldTotals.GetValueOrDefault(s.Year);
            return new YearlyShareDto
            {
                Year = s.Year,
                FirstPriorityCount = s.FirstPriorityCount,
                FieldTotalDemand = fieldTotal,
                MarketSharePct = fieldTotal > 0
                    ? Calc.Round((double)s.FirstPriorityCount / fieldTotal * 100, 2)
                    : 0,
            };
        }).ToList();

        return new ProgramCompetitionTrendDto
        {
            ProgramId = program.Id,
            ProgramName = program.Name,
            FieldId = program.FieldId ?? 0,
            FieldName = program.Field?.Name ?? "",
            Years = years,
        };
    }
}
