using Admissions.Domain.Entities;
using Admissions.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Admissions.Tests.Integration;

public class TestAppFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"AdmissionsTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {

            var stale = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                            d.ServiceType == typeof(AppDbContext) ||
                            d.ServiceType.FullName?.Contains("IDbContextOptionsConfiguration") == true)
                .ToList();
            foreach (var descriptor in stale)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(_dbName));

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            Seed(db);
        });
    }

    private static void Seed(AppDbContext db)
    {
        if (db.Universities.Any())
        {
            return;
        }

        var tsu = new University { Name = "თბილისის სახელმწიფო უნივერსიტეტი", ShortName = "თსუ", Code = "001" };
        var gtu = new University { Name = "საქართველოს ტექნიკური უნივერსიტეტი", ShortName = "სტუ", Code = "003" };

        var hum = new Field { Name = "ჰუმანიტარული მეცნიერებები", Code = "HUM" };
        var tech = new Field { Name = "ტექნიკური მეცნიერებები", Code = "TECH" };

        var philology = new Domain.Entities.Program
        {
            Code = "0010101",
            Name = "ქართული ფილოლოგია",
            University = tsu,
            Field = hum,
            DegreeLevel = "ბაკალავრი",
            YearStats =
            [
                YearStat(2023, 120, 98, 145, 380, 2250m, 22, 18),
                YearStat(2024, 120, 108, 163, 420, 2250m, 25, 20),
                YearStat(2025, 130, 115, 178, 460, 2500m, 28, 22, withBreakdowns: true),
            ],
        };

        var history = new Domain.Entities.Program
        {
            Code = "0010102",
            Name = "ისტორია",
            University = tsu,
            Field = hum,
            DegreeLevel = "ბაკალავრი",
            YearStats =
            [
                YearStat(2023, 80, 55, 48, 165, 2250m, 10, 8),
                YearStat(2024, 80, 52, 45, 152, 2250m, 9, 7),
                YearStat(2025, 80, 52, 43, 148, 2250m, 9, 7),
            ],
        };

        var cs = new Domain.Entities.Program
        {
            Code = "0030101",
            Name = "კომპიუტერული მეცნიერებები",
            University = gtu,
            Field = tech,
            DegreeLevel = "ბაკალავრი",
            YearStats =
            [
                YearStat(2023, 100, 95, 280, 650, 2500m, 28, 22),
                YearStat(2024, 110, 105, 310, 720, 2700m, 32, 25),
                YearStat(2025, 120, 115, 345, 810, 2900m, 36, 28),
            ],
        };

        db.AddRange(tsu, gtu, hum, tech, philology, history, cs);
        db.SaveChanges();
    }

    private static ProgramYearStat YearStat(
        int year, int announced, int enrolled, int fp, int total, decimal fee,
        int grantFull, int grantPartial, bool withBreakdowns = false)
    {
        var stat = new ProgramYearStat
        {
            Year = year,
            AnnouncedPlaces = announced,
            EnrolledCount = enrolled,
            FirstPriorityCount = fp,
            TotalPriorityCount = total,
            AnnualFee = fee,
            GrantFullCount = grantFull,
            GrantPartialCount = grantPartial,
        };

        if (withBreakdowns)
        {
            int remaining = total - fp;
            stat.PriorityBreakdowns.Add(new PriorityBreakdown { Priority = 1, Count = fp });
            for (int p = 2; p <= 10; p++)
            {
                int count = p <= 5 ? remaining / 8 : remaining / 16;
                stat.PriorityBreakdowns.Add(new PriorityBreakdown { Priority = p, Count = count });
            }
        }

        return stat;
    }
}
