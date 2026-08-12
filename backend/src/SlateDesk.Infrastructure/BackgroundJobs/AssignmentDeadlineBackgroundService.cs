using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SlateDesk.Domain.Enums;
using SlateDesk.Infrastructure.Persistence;

namespace SlateDesk.Infrastructure.BackgroundJobs;

public sealed class AssignmentDeadlineBackgroundService
    : BackgroundService
{
    private static readonly TimeSpan Interval =
        TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory
        _scopeFactory;

    private readonly ILogger<
        AssignmentDeadlineBackgroundService>
        _logger;

    public AssignmentDeadlineBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<
            AssignmentDeadlineBackgroundService>
            logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken
            .IsCancellationRequested)
        {
            try
            {
                await CloseExpiredAssignmentsAsync(
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken
                    .IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Automatic assignment closing failed.");
            }

            await Task.Delay(
                Interval,
                stoppingToken);
        }
    }

    private async Task
        CloseExpiredAssignmentsAsync(
            CancellationToken cancellationToken)
    {
        using IServiceScope scope =
            _scopeFactory.CreateScope();

        ApplicationDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<
                    ApplicationDbContext>();

        DateTime utcNow = DateTime.UtcNow;

        int updated =
            await dbContext.Assignments
                .Where(assignment =>
                    assignment.Status ==
                        AssignmentStatus.Published &&
                    assignment.DeadlineUtc <=
                        utcNow)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            assignment =>
                                assignment.Status,
                            AssignmentStatus.Closed)
                        .SetProperty(
                            assignment =>
                                assignment.UpdatedAtUtc,
                            utcNow),
                    cancellationToken);

        if (updated > 0)
        {
            _logger.LogInformation(
                "Automatically closed {AssignmentCount} expired assignments.",
                updated);
        }
    }
}