using System.Text.Json.Serialization;
using Admissions.Api.Middleware;
using Admissions.Application;
using Admissions.Application.Common;
using Admissions.Infrastructure;
using Admissions.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {

        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<AnalyticsOptions>(builder.Configuration.GetSection(AnalyticsOptions.SectionName));

const string FrontendCors = "Frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCors, policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
            ?? ["http://localhost:5173"];
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = 50_000_000);
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 50_000_000);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(FrontendCors);

app.MapControllers();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();

public partial class Program
{
}
