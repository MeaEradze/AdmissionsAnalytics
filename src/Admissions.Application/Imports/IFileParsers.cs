namespace Admissions.Application.Imports;

public sealed record EnrollmentAggregate(
    string UniversityCode,
    string UniversityName,
    string ProgramCode,
    string ProgramName,
    int EnrolledCount,
    int GrantFullCount,
    int GrantPartialCount);

public sealed record EnrollmentParseResult(
    int RowsRead,
    List<EnrollmentAggregate> Aggregates,
    List<string> Errors);

public interface IEnrollmentFileParser
{
    EnrollmentParseResult Parse(Stream file);
}

public sealed record PriorityRow(
    string UniversityCode,
    string UniversityName,
    string ProgramCode,
    string ProgramName,
    IReadOnlyList<int> PriorityCounts,
    int TotalCount);

public sealed record PriorityParseResult(
    int RowsRead,
    List<PriorityRow> Rows,
    List<string> Errors);

public interface IPrioritiesFileParser
{
    PriorityParseResult Parse(Stream file);
}

public sealed record HandbookProgram(
    string UniversityCode,
    string? UniversityName,
    string ProgramCode,
    string ProgramName,
    decimal AnnualFee,
    int AnnouncedPlaces);

public sealed record HandbookParseResult(
    int RowsRead,
    List<HandbookProgram> Programs,
    List<string> Errors);

public interface IHandbookFileParser
{
    HandbookParseResult Parse(Stream file);
}
