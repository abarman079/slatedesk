using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SlateDesk.Application.Assignments.Interfaces;

namespace SlateDesk.Infrastructure.BackgroundJobs;

public sealed class AssignmentDeadlineBackgroundService
    : BackgroundService
{
    private static readonly TimeSpan Interval =
        TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ILogger<
        AssignmentDeadlineBackgroundService> _logger;

    public AssignmentDeadlineBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<AssignmentDeadlineBackgroundService>
            logger)
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
                using IServiceScope scope =
                    _scopeFactory.CreateScope();

                IAssignmentClosingService
                    closingService =
                        scope.ServiceProvider
                            .GetRequiredService<
                                IAssignmentClosingService>();

                int closedCount =
                    await closingService
                        .CloseExpiredAssignmentsAsync(
                            DateTime.UtcNow,
                            stoppingToken);

                if (closedCount > 0)
                {
                    _logger.LogInformation(
                        "Automatically closed {AssignmentCount} expired assignments.",
                        closedCount);
                }
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
}
