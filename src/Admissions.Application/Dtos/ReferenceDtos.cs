using System.Text.Json.Serialization;
using Admissions.Domain.Enums;

namespace Admissions.Application.Dtos;

public class PagedResponse<T>
{
    public List<T> Data { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class UniversityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ShortName { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; set; }
}

public class FieldDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; set; }
}

public class ProgramYearDto
{
    public int Year { get; set; }
    public int AnnouncedPlaces { get; set; }
    public int EnrolledCount { get; set; }
    public int FirstPriorityCount { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TotalPriorityCount { get; set; }

    public decimal AnnualFee { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? GrantFullCount { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? GrantPartialCount { get; set; }
}

public class ProgramListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; set; }

    public int UniversityId { get; set; }
    public string UniversityName { get; set; } = "";
    public int FieldId { get; set; }
    public string FieldName { get; set; } = "";
    public int Year { get; set; }
    public int AnnouncedPlaces { get; set; }
    public int EnrolledCount { get; set; }
    public int FirstPriorityCount { get; set; }
    public decimal AnnualFee { get; set; }

    public double? CompositeScore { get; set; }
    public HealthCategory? Category { get; set; }
}

public class ProgramDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DegreeLevel { get; set; }

    public UniversityDto University { get; set; } = new();
    public FieldDto Field { get; set; } = new();
    public List<ProgramYearDto> YearStats { get; set; } = [];
}

public class CreateUniversityRequest
{
    public string Name { get; set; } = "";
    public string? ShortName { get; set; }
    public string? Code { get; set; }
}

public class CreateFieldRequest
{
    public string Name { get; set; } = "";
    public string? Code { get; set; }
}

public class UpdateFieldRequest
{
    public string Name { get; set; } = "";
    public string? Code { get; set; }
}

public class UpdateProgramYearRequest
{
    public int AnnouncedPlaces { get; set; }
    public int EnrolledCount { get; set; }
    public int FirstPriorityCount { get; set; }
    public int? TotalPriorityCount { get; set; }
    public decimal AnnualFee { get; set; }
    public int? GrantFullCount { get; set; }
    public int? GrantPartialCount { get; set; }
}

public class AssignProgramFieldRequest
{
    public int? FieldId { get; set; }
}
