namespace Admissions.Domain.Entities;

public class University
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? ShortName { get; set; }

    public string? Code { get; set; }

    public ICollection<Program> Programs { get; set; } = new List<Program>();
}
