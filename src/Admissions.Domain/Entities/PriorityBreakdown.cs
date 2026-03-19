namespace Admissions.Domain.Entities;

public class PriorityBreakdown
{
    public int Id { get; set; }

    public int ProgramYearStatId { get; set; }
    public ProgramYearStat ProgramYearStat { get; set; } = null!;

    public int Priority { get; set; }

    public int Count { get; set; }
}
