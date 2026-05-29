using Admissions.Application.Analytics;
using Admissions.Application.Imports;
using Admissions.Domain.Entities;
using Admissions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using DomainProgram = Admissions.Domain.Entities.Program;

namespace Admissions.Tests.Unit;

public class PrioritiesImportJoinTests
{
    private sealed class FakeParser : IPrioritiesFileParser
    {
        private readonly PriorityParseResult _result;
        public FakeParser(params PriorityRow[] rows) =>
            _result = new PriorityParseResult(rows.Length, [.. rows], []);
        public PriorityParseResult Parse(Stream file) => _result;
    }

    private static AppDbContext NewDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"PrioritiesJoin-{Guid.NewGuid()}")
            .Options);

    private static PriorityRow Row(string code, string name, int firstPriority) =>
        new("001", "თსუ", code, name,
            [firstPriority, 0, 0, 0, 0, 0, 0, 0, 0, 0], firstPriority + 100);

    private static (DomainProgram Economics, DomainProgram BusinessAdmin) SeedDriftedPrograms(AppDbContext db)
    {
        var tsu = new University { Name = "თსუ", Code = "001" };

        var economics = new DomainProgram
        {
            Code = "0010136", Name = "ეკონომიკა", University = tsu, DegreeLevel = "ბაკალავრი",
        };
        var businessAdmin = new DomainProgram
        {
            Code = "0010137", Name = "ბიზნესის ადმინისტრირება (180 კრედიტი)",
            University = tsu, DegreeLevel = "ბაკალავრი",
        };
        db.AddRange(tsu, economics, businessAdmin);
        db.SaveChanges();
        return (economics, businessAdmin);
    }

    private static Task<Application.Dtos.ImportResultDto> RunImport(
        AppDbContext db, params PriorityRow[] rows) =>
        new ImportPrioritiesHandler(db, new FakeParser(rows), new ImportGate(), new HealthCache())
            .Handle(new ImportPrioritiesCommand(Stream.Null, 2025), CancellationToken.None);

    [Fact]
    public async Task DriftedCode_JoinsBySuffixTolerantName_NotByCode()
    {
        using var db = NewDb();
        var (economics, businessAdmin) = SeedDriftedPrograms(db);

        var result = await RunImport(db,
            Row("0010136", "ბიზნესის ადმინისტრირება", 694),
            Row("0010135", "ეკონომიკა", 628));

        Assert.Equal(2, result.RowsImported);
        Assert.Empty(result.Errors);

        var economicsStat = db.ProgramYearStats.Single(s => s.ProgramId == economics.Id);
        var businessStat = db.ProgramYearStats.Single(s => s.ProgramId == businessAdmin.Id);
        Assert.Equal(628, economicsStat.FirstPriorityCount);
        Assert.Equal(694, businessStat.FirstPriorityCount);
    }

    [Fact]
    public async Task ExactNameClaim_BlocksCodeFallbackFromStealingTheProgram()
    {
        using var db = NewDb();
        var (economics, _) = SeedDriftedPrograms(db);

        var result = await RunImport(db,
            Row("0010136", "უცნობი პროგრამა", 999),
            Row("0010135", "ეკონომიკა", 628));

        Assert.Equal(1, result.RowsImported);
        Assert.Single(result.Errors);

        var economicsStat = db.ProgramYearStats.Single(s => s.ProgramId == economics.Id);
        Assert.Equal(628, economicsStat.FirstPriorityCount);
    }

    [Fact]
    public async Task UnmatchedRow_IsReported_AndCreatesNoPhantomProgram()
    {
        using var db = NewDb();
        SeedDriftedPrograms(db);
        int programsBefore = db.Programs.Count();

        var result = await RunImport(db, Row("0209999", "არარსებული პროგრამა", 40));

        Assert.Equal(0, result.RowsImported);
        Assert.Contains(result.Errors, e => e.Contains("ვერ მოიძებნა"));
        Assert.Equal(programsBefore, db.Programs.Count());
    }

    [Fact]
    public async Task AmbiguousBaseName_FallsBackToCode()
    {
        using var db = NewDb();
        var tsu = new University { Name = "თსუ", Code = "001" };

        var kutaisi = new DomainProgram
        {
            Code = "0010201", Name = "სამართალი (ქ. ქუთაისი)", University = tsu,
            DegreeLevel = "ბაკალავრი",
        };
        var akhaltsikhe = new DomainProgram
        {
            Code = "0010202", Name = "სამართალი (ქ. ახალციხე)", University = tsu,
            DegreeLevel = "ბაკალავრი",
        };
        db.AddRange(tsu, kutaisi, akhaltsikhe);
        db.SaveChanges();

        var result = await RunImport(db, Row("0010202", "სამართალი", 55));

        Assert.Equal(1, result.RowsImported);
        Assert.Empty(result.Errors);
        var stat = db.ProgramYearStats.Single(s => s.ProgramId == akhaltsikhe.Id);
        Assert.Equal(55, stat.FirstPriorityCount);
    }
}
