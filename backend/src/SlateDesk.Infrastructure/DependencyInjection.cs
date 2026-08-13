using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using SlateDesk.Application.Authentication.Interfaces;
using SlateDesk.Infrastructure.Authentication;
using SlateDesk.Infrastructure.BackgroundJobs;
using SlateDesk.Infrastructure.Identity;
using SlateDesk.Infrastructure.Persistence;
using SlateDesk.Infrastructure.Persistence.Seed;
using SlateDesk.Application.Admin.Interfaces;
using SlateDesk.Infrastructure.Admin;
using SlateDesk.Application.Assignments.Interfaces;
using SlateDesk.Infrastructure.Assignments;
using SlateDesk.Application.Submissions.Interfaces;
using SlateDesk.Infrastructure.Submissions;

namespace SlateDesk.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString =
            configuration.GetConnectionString(
                "DefaultConnection")
            ?? throw new InvalidOperationException(
                "The DefaultConnection connection string is missing.");

        services.AddDataProtection();

        services.AddDbContext<ApplicationDbContext>(
            options =>
            {
                options
                    .UseNpgsql(connectionString)
                    .UseSeeding((context, _) =>
                    {
                        DatabaseSeedData.Seed(context);
                    })
                    .UseAsyncSeeding(
                        async (
                            context,
                            _,
                            cancellationToken) =>
                        {
                            await DatabaseSeedData.SeedAsync(
                                context,
                                cancellationToken);
                        });
            });

        services
            .AddIdentityCore<ApplicationUser>(
                options =>
                {
                    options.User.RequireUniqueEmail = true;

                    options.SignIn.RequireConfirmedEmail =
                        false;

                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric =
                        true;

                    options.Lockout.MaxFailedAccessAttempts =
                        5;

                    options.Lockout.DefaultLockoutTimeSpan =
                        TimeSpan.FromMinutes(10);
                })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<
                ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        JwtOptions jwtOptions =
            configuration
                .GetSection(JwtOptions.SectionName)
                .Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                "The Jwt configuration section is missing.");

        if (string.IsNullOrWhiteSpace(
                jwtOptions.SigningKey) ||
            Encoding.UTF8.GetByteCount(
                jwtOptions.SigningKey) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey must contain at least 32 bytes.");
        }

        services
            .AddOptions<JwtOptions>()
            .Bind(
                configuration.GetSection(
                    JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddAuthentication(
                options =>
                {
                    options.DefaultAuthenticateScheme =
                        JwtBearerDefaults.AuthenticationScheme;

                    options.DefaultChallengeScheme =
                        JwtBearerDefaults.AuthenticationScheme;
                })
            .AddJwtBearer(
                options =>
                {
                    options.MapInboundClaims = false;
                    options.SaveToken = false;

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer =
                                jwtOptions.Issuer,

                            ValidateAudience = true,
                            ValidAudience =
                                jwtOptions.Audience,

                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(
                                        jwtOptions.SigningKey)),

                            ValidateLifetime = true,
                            RequireExpirationTime = true,

                            ClockSkew =
                                TimeSpan.FromSeconds(30),

                            NameClaimType =
                                ClaimTypes.Name,

                            RoleClaimType =
                                ClaimTypes.Role
                        };

                    options.Events =
                        new JwtBearerEvents
                        {
                            OnTokenValidated =
                                async context =>
                                {
                                    string? userId =
                                        context.Principal?
                                            .FindFirst(
                                                ClaimTypes
                                                    .NameIdentifier)?
                                            .Value;

                                    if (string.IsNullOrWhiteSpace(
                                            userId))
                                    {
                                        context.Fail(
                                            "The token has no user identifier.");

                                        return;
                                    }

                                    UserManager<ApplicationUser>
                                        userManager =
                                            context.HttpContext
                                                .RequestServices
                                                .GetRequiredService<
                                                    UserManager<
                                                        ApplicationUser>>();

                                    ApplicationUser? user =
                                        await userManager
                                            .FindByIdAsync(
                                                userId);

                                    if (user is null ||
                                        !user.IsActive)
                                    {
                                        context.Fail(
                                            "The user is inactive or unavailable.");
                                    }
                                },

                            OnChallenge =
                                async context =>
                                {
                                    if (context.Response
                                        .HasStarted)
                                    {
                                        return;
                                    }

                                    context.HandleResponse();

                                    await JwtProblemDetailsWriter
                                        .WriteAsync(
                                            context.HttpContext,
                                            StatusCodes
                                                .Status401Unauthorized,
                                            "https://slatedesk.local/errors/unauthorized",
                                            "Authentication required",
                                            "A valid access token is required.");
                                },

                            OnForbidden =
                                async context =>
                                {
                                    await JwtProblemDetailsWriter
                                        .WriteAsync(
                                            context.HttpContext,
                                            StatusCodes
                                                .Status403Forbidden,
                                            "https://slatedesk.local/errors/forbidden",
                                            "Access forbidden",
                                            "Your account does not have permission to perform this action.");
                                }
                        };
                });

        services.AddScoped<
            ITokenService,
            JwtTokenService>();

        services.AddScoped<
            IAuthenticationService,
            AuthenticationService>();

        services.AddHostedService<
            RefreshTokenCleanupService>();

        services
            .AddHealthChecks()
            .AddCheck(
                "api",
                () => HealthCheckResult.Healthy(
                    "SlateDesk API is running."))
            .AddDbContextCheck<
                ApplicationDbContext>(
                    "postgresql");

        services.AddScoped<
            IAdminUserService,
            AdminUserService>();
        services.AddScoped<
            IAdminAcademicService,
            AdminAcademicService>();

        services.AddScoped<
            IAdminSetupService,
            AdminSetupService>();

        services.AddScoped<
            IAssignmentDeadlinePolicy,
            AssignmentDeadlinePolicy>();

        services.AddScoped<
            IAssignmentService,
            AssignmentService>();

        services.AddHostedService<
            AssignmentDeadlineBackgroundService>();

        services.AddScoped<
            ISubmissionService,
            SubmissionService>();

        services.AddScoped<
            IAssignmentClosingService,
            AssignmentClosingService>();

        services.AddScoped<
            IAdminOverviewService,
            AdminOverviewService>();

        return services;
    }
}