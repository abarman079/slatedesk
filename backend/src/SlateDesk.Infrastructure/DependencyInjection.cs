using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SlateDesk.Infrastructure.Identity;
using SlateDesk.Infrastructure.Persistence;
using SlateDesk.Infrastructure.Persistence.Seed;

namespace SlateDesk.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "The DefaultConnection connection string is missing.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options
                .UseNpgsql(connectionString)
                .UseSeeding((context, _) =>
                {
                    DatabaseSeedData.Seed(context);
                })
                .UseAsyncSeeding(
                    async (context, _, cancellationToken) =>
                    {
                        await DatabaseSeedData.SeedAsync(
                            context,
                            cancellationToken);
                    });
        });
        services.AddDataProtection();

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.SignIn.RequireConfirmedEmail = false;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(10);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services
            .AddHealthChecks()
            .AddCheck(
                "api",
                () => HealthCheckResult.Healthy(
                    "SlateDesk API is running."))
            .AddDbContextCheck<ApplicationDbContext>(
                "postgresql");

        return services;
    }
}