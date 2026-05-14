using Admissions.Application.Analytics;
using Admissions.Application.Common.Behaviors;
using Admissions.Application.Imports;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Admissions.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddSingleton<HealthCache>();
        services.AddSingleton<ImportGate>();
        return services;
    }
}
