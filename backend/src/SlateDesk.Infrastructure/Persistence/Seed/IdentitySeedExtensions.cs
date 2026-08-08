using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlateDesk.Domain.Constants;
using SlateDesk.Infrastructure.Identity;

namespace SlateDesk.Infrastructure.Persistence.Seed;

public static class IdentitySeedExtensions
{
    public static async Task SeedIdentityDataAsync(
        this IServiceProvider services,
        IConfiguration configuration)
    {
        using IServiceScope scope =
            services.CreateScope();

        RoleManager<IdentityRole> roleManager =
            scope.ServiceProvider
                .GetRequiredService<
                    RoleManager<IdentityRole>>();

        UserManager<ApplicationUser> userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        ILogger logger =
            scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("IdentitySeeder");

        foreach (string roleName in AppRoles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            IdentityResult roleResult =
                await roleManager.CreateAsync(
                    new IdentityRole(roleName));

            EnsureIdentityResultSucceeded(
                roleResult,
                $"creating the {roleName} role");
        }

        bool seedEnabled =
            configuration.GetValue<bool>(
                "DemoAccounts:SeedEnabled");

        if (!seedEnabled)
        {
            logger.LogInformation(
                "Demo-account seeding is disabled.");

            return;
        }

        string demoPassword =
            configuration["DemoAccounts:Password"]
            ?? throw new InvalidOperationException(
                "DemoAccounts:Password is missing.");

        DemoAccount[] demoAccounts =
        [
            new(
                configuration[
                    "DemoAccounts:Admin:Email"]
                    ?? "admin@slatedesk.local",
                configuration[
                    "DemoAccounts:Admin:FullName"]
                    ?? "Amina Rahman",
                AppRoles.Admin),

            new(
                configuration[
                    "DemoAccounts:Teacher:Email"]
                    ?? "teacher@slatedesk.local",
                configuration[
                    "DemoAccounts:Teacher:FullName"]
                    ?? "Farhan Ahmed",
                AppRoles.Teacher),

            new(
                configuration[
                    "DemoAccounts:Student:Email"]
                    ?? "student@slatedesk.local",
                configuration[
                    "DemoAccounts:Student:FullName"]
                    ?? "Nadia Islam",
                AppRoles.Student)
        ];

        foreach (DemoAccount account in demoAccounts)
        {
            await CreateOrUpdateDemoUserAsync(
                userManager,
                account,
                demoPassword);
        }

        logger.LogInformation(
            "Identity roles and demo accounts are ready.");
    }

    private static async Task CreateOrUpdateDemoUserAsync(
        UserManager<ApplicationUser> userManager,
        DemoAccount account,
        string password)
    {
        string normalizedEmail =
            account.Email.ToUpperInvariant();

        ApplicationUser? user =
            await userManager.Users
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(
                    existingUser =>
                        existingUser.NormalizedEmail ==
                        normalizedEmail);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = account.Email,
                Email = account.Email,
                FullName = account.FullName,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            IdentityResult createResult =
                await userManager.CreateAsync(
                    user,
                    password);

            EnsureIdentityResultSucceeded(
                createResult,
                $"creating {account.Email}");
        }
        else
        {
            bool requiresUpdate =
                user.FullName != account.FullName ||
                !user.IsActive ||
                !user.EmailConfirmed;

            if (requiresUpdate)
            {
                user.FullName = account.FullName;
                user.IsActive = true;
                user.EmailConfirmed = true;
                user.UpdatedAtUtc = DateTime.UtcNow;

                IdentityResult updateResult =
                    await userManager.UpdateAsync(user);

                EnsureIdentityResultSucceeded(
                    updateResult,
                    $"updating {account.Email}");
            }
        }

        bool hasRequiredRole =
            await userManager.IsInRoleAsync(
                user,
                account.Role);

        if (!hasRequiredRole)
        {
            IdentityResult roleResult =
                await userManager.AddToRoleAsync(
                    user,
                    account.Role);

            EnsureIdentityResultSucceeded(
                roleResult,
                $"assigning {account.Role} to {account.Email}");
        }
    }

    private static void EnsureIdentityResultSucceeded(
        IdentityResult result,
        string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        string errors = string.Join(
            "; ",
            result.Errors.Select(error =>
                $"{error.Code}: {error.Description}"));

        throw new InvalidOperationException(
            $"Identity failed while {operation}. {errors}");
    }

    private sealed record DemoAccount(
        string Email,
        string FullName,
        string Role);
}