using Admissions.Application.Common;
using Admissions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Admissions.Application.Imports;

public sealed class ImportContext
{
    private readonly IAppDbContext _db;
    private readonly Dictionary<string, University> _universitiesByCode;
    private readonly Dictionary<string, Domain.Entities.Program> _programsByCode;
    private readonly Dictionary<int, string> _universityCodeById;

    private readonly Dictionary<(string UniCode, string Name), Domain.Entities.Program?> _programsByName;

    private readonly Dictionary<(string UniCode, string Name), Domain.Entities.Program?> _programsByBaseName;
    private readonly int _year;

    private ImportContext(
        IAppDbContext db,
        int year,
        Dictionary<string, University> universities,
        Dictionary<string, Domain.Entities.Program> programs)
    {
        _db = db;
        _year = year;
        _universitiesByCode = universities;
        _programsByCode = programs;
        _universityCodeById = universities.ToDictionary(kv => kv.Value.Id, kv => kv.Key);

        _programsByName = [];
        _programsByBaseName = [];
        foreach (var program in programs.Values)
        {
            IndexByName(program);
        }
    }

    public static async Task<ImportContext> CreateAsync(
        IAppDbContext db,
        int year,
        bool includeBreakdowns,
        CancellationToken ct)
    {
        var universities = await db.Universities.ToListAsync(ct);

        IQueryable<Domain.Entities.Program> programsQuery = includeBreakdowns
            ? db.Programs
                .Include(p => p.YearStats.Where(s => s.Year == year))
                .ThenInclude(s => s.PriorityBreakdowns)
            : db.Programs
                .Include(p => p.YearStats.Where(s => s.Year == year));

        var programs = await programsQuery.ToListAsync(ct);

        return new ImportContext(
            db,
            year,
            universities
                .Where(u => !string.IsNullOrEmpty(u.Code))
                .GroupBy(u => u.Code!)
                .ToDictionary(g => g.Key, g => g.First()),
            programs.ToDictionary(p => p.Code));
    }

    public University GetOrCreateUniversity(string code, string? name)
    {
        if (_universitiesByCode.TryGetValue(code, out var existing))
        {
            return existing;
        }

        var uni = new University
        {
            Code = code,
            Name = string.IsNullOrWhiteSpace(name) ? $"უსდ {code}" : name.Trim(),
        };
        _db.Universities.Add(uni);
        _universitiesByCode[code] = uni;
        return uni;
    }

    public Domain.Entities.Program GetOrCreateProgram(string code, string name, University university)
    {
        if (_programsByCode.TryGetValue(code, out var existing))
        {
            return existing;
        }

        var program = new Domain.Entities.Program
        {
            Code = code,
            Name = name.Trim(),
            University = university,
            DegreeLevel = "ბაკალავრი",
        };
        _db.Programs.Add(program);
        _programsByCode[code] = program;
        IndexByName(program, university.Code);
        return program;
    }

    public Domain.Entities.Program? TryFindByName(string universityCode, string name)
    {
        return _programsByName.TryGetValue((universityCode, NormalizeName(name)), out var program)
            ? program
            : null;
    }

    public Domain.Entities.Program? TryFindByBaseName(string universityCode, string name)
    {
        if (_programsByBaseName.TryGetValue((universityCode, NormalizeName(name)), out var bySuffixedDb)
            && bySuffixedDb is not null)
        {
            return bySuffixedDb;
        }

        var strippedFileName = NormalizeName(StripParentheticals(name));
        if (strippedFileName.Length > 0 && strippedFileName != NormalizeName(name)
            && _programsByName.TryGetValue((universityCode, strippedFileName), out var byStrippedFile))
        {
            return byStrippedFile;
        }

        return null;
    }

    public Domain.Entities.Program? TryFindByCode(string code) =>
        _programsByCode.TryGetValue(code, out var program) ? program : null;

    public IEnumerable<ProgramYearStat> StatsForYear =>
        _programsByCode.Values.SelectMany(p => p.YearStats).Where(s => s.Year == _year);

    public ProgramYearStat GetOrCreateStat(Domain.Entities.Program program)
    {
        var stat = program.YearStats.FirstOrDefault(s => s.Year == _year);
        if (stat is null)
        {
            stat = new ProgramYearStat { Year = _year };
            program.YearStats.Add(stat);
        }

        return stat;
    }

    public Task<int> SaveAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

    private void IndexByName(Domain.Entities.Program program, string? uniCode = null)
    {
        uniCode ??= program.University?.Code
            ?? (_universityCodeById.TryGetValue(program.UniversityId, out var code) ? code : null);
        if (uniCode is null)
        {
            return;
        }

        var normalized = NormalizeName(program.Name);
        var key = (uniCode, normalized);
        _programsByName[key] = _programsByName.ContainsKey(key)
            ? null
            : program;

        var baseName = NormalizeName(StripParentheticals(program.Name));
        if (baseName.Length > 0 && baseName != normalized)
        {
            var baseKey = (uniCode, baseName);
            _programsByBaseName[baseKey] = _programsByBaseName.ContainsKey(baseKey)
                ? null
                : program;
        }
    }

    private static string NormalizeName(string name) =>
        string.Join(' ', name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static string StripParentheticals(string name)
    {
        Span<char> buffer = name.Length <= 512 ? stackalloc char[name.Length] : new char[name.Length];
        int length = 0;
        int depth = 0;
        foreach (var ch in name)
        {
            if (ch == '(') { depth++; continue; }
            if (ch == ')') { if (depth > 0) depth--; continue; }
            if (depth == 0) { buffer[length++] = ch; }
        }

        return new string(buffer[..length]);
    }
}
