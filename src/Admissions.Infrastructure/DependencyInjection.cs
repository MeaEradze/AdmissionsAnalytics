using Admissions.Application.Common;
using Admissions.Application.Imports;
using Admissions.Infrastructure.Parsers;
using Admissions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Admissions.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddSingleton<IEnrollmentFileParser, EnrollmentExcelParser>();
        services.AddSingleton<IPrioritiesFileParser, PrioritiesExcelParser>();
        services.AddSingleton<IHandbookFileParser, HandbookPdfParser>();

        return services;
    }
}
