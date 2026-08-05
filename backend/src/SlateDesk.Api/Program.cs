using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using SlateDesk.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks(
    "/api/v1/health",
    new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                status = report.Status.ToString(),
                application = "SlateDesk API",
                timestampUtc = DateTime.UtcNow,
                totalDurationMs = Math.Round(
                    report.TotalDuration.TotalMilliseconds,
                    2),
                checks = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    durationMs = Math.Round(
                        entry.Value.Duration.TotalMilliseconds,
                        2)
                })
            });
        }
    });

app.Run();

// Required for integration tests in a later phase.
public partial class Program
{
}