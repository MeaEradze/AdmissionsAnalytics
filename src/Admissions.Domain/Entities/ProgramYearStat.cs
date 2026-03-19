namespace Admissions.Domain.Entities;

public class ProgramYearStat
{
    public int Id { get; set; }

    public int ProgramId { get; set; }
    public Program Program { get; set; } = null!;

    public int Year { get; set; }

    public int AnnouncedPlaces { get; set; }

    public decimal AnnualFee { get; set; }

    public int EnrolledCount { get; set; }

    public int? GrantFullCount { get; set; }
    public int? GrantPartialCount { get; set; }

    public int FirstPriorityCount { get; set; }

    public int? TotalPriorityCount { get; set; }

    public ICollection<PriorityBreakdown> PriorityBreakdowns { get; set; } = new List<PriorityBreakdown>();
}
