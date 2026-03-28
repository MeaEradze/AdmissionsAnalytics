using Admissions.Application.Common;
using Admissions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Admissions.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<University> Universities => Set<University>();
    public DbSet<Field> Fields => Set<Field>();
    public DbSet<Program> Programs => Set<Program>();
    public DbSet<ProgramYearStat> ProgramYearStats => Set<ProgramYearStat>();
    public DbSet<PriorityBreakdown> PriorityBreakdowns => Set<PriorityBreakdown>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<University>(b =>
        {
            b.Property(u => u.Name).IsRequired().HasMaxLength(500);
            b.Property(u => u.ShortName).HasMaxLength(100);
            b.Property(u => u.Code).HasMaxLength(10);
            b.HasIndex(u => u.Code).IsUnique().HasFilter("[Code] IS NOT NULL");
        });

        modelBuilder.Entity<Field>(b =>
        {
            b.Property(f => f.Name).IsRequired().HasMaxLength(300);
            b.Property(f => f.Code).HasMaxLength(20);
        });

        modelBuilder.Entity<Domain.Entities.Program>(b =>
        {
            b.Property(p => p.Code).IsRequired().HasMaxLength(20);
            b.HasIndex(p => p.Code).IsUnique();

            b.Property(p => p.Name).IsRequired().HasMaxLength(2000);
            b.Property(p => p.DegreeLevel).HasMaxLength(100);

            b.HasOne(p => p.University)
                .WithMany(u => u.Programs)
                .HasForeignKey(p => p.UniversityId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(p => p.Field)
                .WithMany(f => f.Programs)
                .HasForeignKey(p => p.FieldId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ProgramYearStat>(b =>
        {
            b.HasIndex(s => new { s.ProgramId, s.Year }).IsUnique();
            b.Property(s => s.AnnualFee).HasPrecision(12, 2);

            b.HasOne(s => s.Program)
                .WithMany(p => p.YearStats)
                .HasForeignKey(s => s.ProgramId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PriorityBreakdown>(b =>
        {
            b.HasIndex(x => new { x.ProgramYearStatId, x.Priority }).IsUnique();

            b.HasOne(x => x.ProgramYearStat)
                .WithMany(s => s.PriorityBreakdowns)
                .HasForeignKey(x => x.ProgramYearStatId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
