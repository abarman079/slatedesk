using Microsoft.EntityFrameworkCore;
using SlateDesk.Domain.Entities;

namespace SlateDesk.Infrastructure.Persistence.Seed;

internal static class DatabaseSeedData
{
    public static void Seed(DbContext context)
    {
        foreach (var setting in CreateDefaultSettings())
        {
            bool alreadyExists = context.Set<AppSetting>()
                .Any(existing => existing.Key == setting.Key);

            if (!alreadyExists)
            {
                context.Set<AppSetting>().Add(setting);
            }
        }

        if (context.ChangeTracker.HasChanges())
        {
            context.SaveChanges();
        }
    }

    public static async Task SeedAsync(
        DbContext context,
        CancellationToken cancellationToken)
    {
        foreach (var setting in CreateDefaultSettings())
        {
            bool alreadyExists = await context.Set<AppSetting>()
                .AnyAsync(
                    existing => existing.Key == setting.Key,
                    cancellationToken);

            if (!alreadyExists)
            {
                await context.Set<AppSetting>()
                    .AddAsync(setting, cancellationToken);
            }
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static AppSetting[] CreateDefaultSettings()
    {
        return
        [
            new AppSetting
            {
                Key = "InstitutionName",
                Value = "SlateDesk Demo College",
                Description = "Displayed institution name."
            },
            new AppSetting
            {
                Key = "DefaultAllowLateSubmission",
                Value = "false",
                Description = "Default late-submission policy."
            },
            new AppSetting
            {
                Key = "DefaultAllowResubmission",
                Value = "true",
                Description = "Default resubmission policy."
            }
        ];
    }
}