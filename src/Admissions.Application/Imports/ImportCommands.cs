using Admissions.Application.Analytics;
using Admissions.Application.Common;
using Admissions.Application.Dtos;
using Admissions.Domain.Entities;
using MediatR;

namespace Admissions.Application.Imports;

internal static class ImportErrors
{
    private const int MaxErrors = 100;

    public static List<string> Cap(List<string> errors)
    {
        if (errors.Count <= MaxErrors)
        {
            return errors;
        }

        var capped = errors.Take(MaxErrors).ToList();
        capped.Add($"... და კიდევ {errors.Count - MaxErrors} შეცდომა");
        return capped;
    }
}

public sealed record ImportEnrollmentsCommand(Stream File, int Year) : IRequest<ImportResultDto>;

public sealed class ImportEnrollmentsHandler : IRequestHandler<ImportEnrollmentsCommand, ImportResultDto>
{
    private readonly IAppDbContext _db;
    private readonly IEnrollmentFileParser _parser;
    private readonly ImportGate _gate;
    private readonly HealthCache _healthCache;

    public ImportEnrollmentsHandler(
        IAppDbContext db, IEnrollmentFileParser parser, ImportGate gate, HealthCache healthCache)
    {
        _db = db;
        _parser = parser;
        _gate = gate;
        _healthCache = healthCache;
    }

    public async Task<ImportResultDto> Handle(ImportEnrollmentsCommand cmd, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var parsed = _parser.Parse(cmd.File);
            var ctx = await ImportContext.CreateAsync(_db, cmd.Year, includeBreakdowns: false, ct);

            int imported = 0;
            foreach (var agg in parsed.Aggregates)
            {
                var uni = ctx.GetOrCreateUniversity(agg.UniversityCode, agg.UniversityName);
                var program = ctx.GetOrCreateProgram(agg.ProgramCode, agg.ProgramName, uni);
                var stat = ctx.GetOrCreateStat(program);

                stat.EnrolledCount = agg.EnrolledCount;
                stat.GrantFullCount = agg.GrantFullCount;
                stat.GrantPartialCount = agg.GrantPartialCount;
                imported++;
            }

            await ctx.SaveAsync(ct);
            _healthCache.Clear();

            return new ImportResultDto
            {
                RowsRead = parsed.RowsRead,
                RowsImported = imported,
                Errors = ImportErrors.Cap(parsed.Errors),
                Year = cmd.Year,
            };
        }
        finally
        {
            _gate.Release();
        }
    }
}

public sealed record ImportPrioritiesCommand(Stream File, int Year) : IRequest<ImportResultDto>;

public sealed class ImportPrioritiesHandler : IRequestHandler<ImportPrioritiesCommand, ImportResultDto>
{
    private readonly IAppDbContext _db;
    private readonly IPrioritiesFileParser _parser;
    private readonly ImportGate _gate;
    private readonly HealthCache _healthCache;

    public ImportPrioritiesHandler(
        IAppDbContext db, IPrioritiesFileParser parser, ImportGate gate, HealthCache healthCache)
    {
        _db = db;
        _parser = parser;
        _gate = gate;
        _healthCache = healthCache;
    }

    public async Task<ImportResultDto> Handle(ImportPrioritiesCommand cmd, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var parsed = _parser.Parse(cmd.File);
            var ctx = await ImportContext.CreateAsync(_db, cmd.Year, includeBreakdowns: true, ct);

            foreach (var stale in ctx.StatsForYear)
            {
                stale.FirstPriorityCount = 0;
                stale.TotalPriorityCount = null;
                stale.PriorityBreakdowns.Clear();
            }

            var errors = new List<string>(parsed.Errors);
            var claimed = new HashSet<Domain.Entities.Program>();
            var pending = new List<PriorityRow>();
            int imported = 0;

            foreach (var row in parsed.Rows)
            {
                ctx.GetOrCreateUniversity(row.UniversityCode, row.UniversityName);

                var program = ctx.TryFindByName(row.UniversityCode, row.ProgramName);
                if (program is null)
                {
                    pending.Add(row);
                    continue;
                }

                if (!claimed.Add(program))
                {
                    errors.Add(DuplicateTargetError(row));
                    continue;
                }

                Apply(ctx, program, row);
                imported++;
            }

            foreach (var row in pending)
            {
                var program = ctx.TryFindByBaseName(row.UniversityCode, row.ProgramName);
                if (program is not null && claimed.Contains(program))
                {
                    program = null;
                }

                if (program is null)
                {
                    program = ctx.TryFindByCode(row.ProgramCode);
                    if (program is not null && claimed.Contains(program))
                    {
                        errors.Add(DuplicateTargetError(row));
                        continue;
                    }
                }

                if (program is null)
                {
                    errors.Add(
                        $"პროგრამა ვერ მოიძებნა: „{row.ProgramName}“ (კოდი {row.ProgramCode}, " +
                        $"უსდ {row.UniversityCode}) — პრიორიტეტების სტრიქონი გამოტოვებულია.");
                    continue;
                }

                claimed.Add(program);
                Apply(ctx, program, row);
                imported++;
            }

            await ctx.SaveAsync(ct);
            _healthCache.Clear();

            return new ImportResultDto
            {
                RowsRead = parsed.RowsRead,
                RowsImported = imported,
                Errors = ImportErrors.Cap(errors),
                Year = cmd.Year,
            };
        }
        finally
        {
            _gate.Release();
        }
    }

    private static void Apply(ImportContext ctx, Domain.Entities.Program program, PriorityRow row)
    {
        var stat = ctx.GetOrCreateStat(program);

        stat.FirstPriorityCount = row.PriorityCounts.Count > 0 ? row.PriorityCounts[0] : 0;
        stat.TotalPriorityCount = row.TotalCount;

        stat.PriorityBreakdowns.Clear();
        for (int i = 0; i < row.PriorityCounts.Count; i++)
        {
            stat.PriorityBreakdowns.Add(new PriorityBreakdown
            {
                Priority = i + 1,
                Count = row.PriorityCounts[i],
            });
        }
    }

    private static string DuplicateTargetError(PriorityRow row) =>
        $"სტრიქონი „{row.ProgramName}“ (კოდი {row.ProgramCode}): სამიზნე პროგრამა უკვე " +
        "სხვა სტრიქონს შეესაბამება — გამოტოვებულია.";
}

public sealed record ImportHandbookCommand(Stream File, int Year) : IRequest<ImportResultDto>;

public sealed class ImportHandbookHandler : IRequestHandler<ImportHandbookCommand, ImportResultDto>
{
    private readonly IAppDbContext _db;
    private readonly IHandbookFileParser _parser;
    private readonly ImportGate _gate;
    private readonly HealthCache _healthCache;

    public ImportHandbookHandler(
        IAppDbContext db, IHandbookFileParser parser, ImportGate gate, HealthCache healthCache)
    {
        _db = db;
        _parser = parser;
        _gate = gate;
        _healthCache = healthCache;
    }

    public async Task<ImportResultDto> Handle(ImportHandbookCommand cmd, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var parsed = _parser.Parse(cmd.File);
            var ctx = await ImportContext.CreateAsync(_db, cmd.Year, includeBreakdowns: false, ct);

            int imported = 0;
            foreach (var entry in parsed.Programs)
            {
                var uni = ctx.GetOrCreateUniversity(entry.UniversityCode, entry.UniversityName);
                var program = ctx.GetOrCreateProgram(entry.ProgramCode, entry.ProgramName, uni);
                var stat = ctx.GetOrCreateStat(program);

                stat.AnnouncedPlaces = entry.AnnouncedPlaces;
                stat.AnnualFee = entry.AnnualFee;
                imported++;
            }

            await ctx.SaveAsync(ct);
            _healthCache.Clear();

            return new ImportResultDto
            {
                RowsRead = parsed.RowsRead,
                RowsImported = imported,
                Errors = ImportErrors.Cap(parsed.Errors),
                Year = cmd.Year,
            };
        }
        finally
        {
            _gate.Release();
        }
    }
}
