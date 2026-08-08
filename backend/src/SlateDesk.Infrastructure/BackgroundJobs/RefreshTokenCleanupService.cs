using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SlateDesk.Infrastructure.Persistence;

namespace SlateDesk.Infrastructure.BackgroundJobs;

public sealed class RefreshTokenCleanupService
    : BackgroundService
{
    private static readonly TimeSpan CleanupInterval =
        TimeSpan.FromHours(6);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RefreshTokenCleanupService> _logger;

    public RefreshTokenCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<RefreshTokenCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RemoveExpiredTokensAsync(
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Refresh-token cleanup failed.");
            }

            await Task.Delay(
                CleanupInterval,
                stoppingToken);
        }
    }

    private async Task RemoveExpiredTokensAsync(
        CancellationToken cancellationToken)
    {
        using IServiceScope scope =
            _scopeFactory.CreateScope();

        ApplicationDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<
                    ApplicationDbContext>();

        DateTime retentionCutoff =
            DateTime.UtcNow.AddDays(-7);

        int deletedCount =
            await dbContext.RefreshTokens
                .Where(token =>
                    token.ExpiresAtUtc <
                    retentionCutoff)
                .ExecuteDeleteAsync(
                    cancellationToken);

        if (deletedCount > 0)
        {
            _logger.LogInformation(
                "Removed {TokenCount} expired refresh tokens.",
                deletedCount);
        }
    }
}