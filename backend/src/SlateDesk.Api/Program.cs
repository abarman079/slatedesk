using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using SlateDesk.Api.Errors;
using SlateDesk.Domain.Constants;
using SlateDesk.Infrastructure;
using SlateDesk.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails(
    options =>
    {
        options.CustomizeProblemDetails =
            context =>
            {
                context.ProblemDetails
                    .Extensions["traceId"] =
                    Activity.Current?.Id ??
                    context.HttpContext.TraceIdentifier;
            };
    });

builder.Services.AddExceptionHandler<
    GlobalExceptionHandler>();

builder.Services.AddControllers();

builder.Services.Configure<ApiBehaviorOptions>(
    options =>
    {
        options.InvalidModelStateResponseFactory =
            context =>
            {
                var problemDetails =
                    new ValidationProblemDetails(
                        context.ModelState)
                    {
                        Type =
                            "https://slatedesk.local/errors/validation",
                        Title = "Validation failed",
                        Status =
                            StatusCodes.Status400BadRequest,
                        Detail =
                            "One or more validation errors occurred.",
                        Instance =
                            context.HttpContext.Request.Path
                    };

                problemDetails.Extensions["traceId"] =
                    Activity.Current?.Id ??
                    context.HttpContext.TraceIdentifier;

                var result =
                    new BadRequestObjectResult(
                        problemDetails);

                result.ContentTypes.Add(
                    "application/problem+json");

                return result;
            };
    });

builder.Services.AddInfrastructure(
    builder.Configuration);

builder.Services.AddAuthorization(
    options =>
    {
        options.AddPolicy(
            AppPolicies.AdminOnly,
            policy =>
                policy.RequireRole(
                    AppRoles.Admin));

        options.AddPolicy(
            AppPolicies.TeacherOnly,
            policy =>
                policy.RequireRole(
                    AppRoles.Teacher));

        options.AddPolicy(
            AppPolicies.StudentOnly,
            policy =>
                policy.RequireRole(
                    AppRoles.Student));
    });

string[] allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
    ?? ["http://localhost:3000"];

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            "SlateDeskFrontend",
            policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(
    options =>
    {
        options.SwaggerDoc(
            "v1",
            new OpenApiInfo
            {
                Title = "SlateDesk API",
                Version = "v1",
                Description =
                    "Role-based Assignment and Submission Management API."
            });

        options.AddSecurityDefinition(
            "bearer",
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description =
                    "Paste the JWT access token only. Do not include the word Bearer."
            });

        options.AddSecurityRequirement(
            document =>
                new OpenApiSecurityRequirement
                {
                    [
                        new OpenApiSecuritySchemeReference(
                            "bearer",
                            document)
                    ] = []
                });
    });

var app = builder.Build();

app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(
        options =>
        {
            options.SwaggerEndpoint(
                "/swagger/v1/swagger.json",
                "SlateDesk API v1");

            options.DocumentTitle =
                "SlateDesk API";
        });
}

app.Use(
    async (context, next) =>
    {
        context.Response.Headers[
            "X-Content-Type-Options"] =
            "nosniff";

        context.Response.Headers[
            "X-Frame-Options"] =
            "DENY";

        context.Response.Headers[
            "Referrer-Policy"] =
            "no-referrer";

        await next();
    });

app.UseHttpsRedirection();

app.UseCors("SlateDeskFrontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks(
    "/api/v1/health",
    new HealthCheckOptions
    {
        ResponseWriter =
            async (context, report) =>
            {
                context.Response.ContentType =
                    "application/json";

                await context.Response.WriteAsJsonAsync(
                    new
                    {
                        status =
                            report.Status.ToString(),
                        application =
                            "SlateDesk API",
                        timestampUtc =
                            DateTime.UtcNow,
                        totalDurationMs =
                            Math.Round(
                                report.TotalDuration
                                    .TotalMilliseconds,
                                2),
                        checks =
                            report.Entries.Select(
                                entry => new
                                {
                                    name = entry.Key,
                                    status =
                                        entry.Value.Status
                                            .ToString(),
                                    description =
                                        entry.Value.Description,
                                    durationMs =
                                        Math.Round(
                                            entry.Value.Duration
                                                .TotalMilliseconds,
                                            2)
                                })
                    });
            }
    });

await app.Services.SeedIdentityDataAsync(
    builder.Configuration);

app.Run();

public partial class Program
{
}