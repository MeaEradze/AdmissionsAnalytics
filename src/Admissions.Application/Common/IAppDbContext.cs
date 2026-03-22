using Admissions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Admissions.Application.Common;

public interface IAppDbContext
{
    DbSet<University> Universities { get; }
    DbSet<Field> Fields { get; }
    DbSet<Domain.Entities.Program> Programs { get; }
    DbSet<ProgramYearStat> ProgramYearStats { get; }
    DbSet<PriorityBreakdown> PriorityBreakdowns { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
