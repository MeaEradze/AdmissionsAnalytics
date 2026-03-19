namespace Admissions.Domain.Entities;

public class Program
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int UniversityId { get; set; }
    public University University { get; set; } = null!;

    public int? FieldId { get; set; }
    public Field? Field { get; set; }

    public string? DegreeLevel { get; set; }

    public ICollection<ProgramYearStat> YearStats { get; set; } = new List<ProgramYearStat>();
}
