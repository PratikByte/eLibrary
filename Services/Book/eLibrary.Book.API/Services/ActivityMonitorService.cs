using eLibrary.Book.Application.Interfaces.Repositories;

namespace eLibrary.Book.API.Services;

public class ActivityMonitorService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(5);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ActivityMonitorService> _logger;

    public ActivityMonitorService(
        IServiceScopeFactory scopeFactory,
        ILogger<ActivityMonitorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var historyBackfilled = false;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var activityRepository = scope.ServiceProvider.GetRequiredService<IActivityRepository>();
                if (!historyBackfilled)
                {
                    await activityRepository.BackfillHistoryAsync(stoppingToken);
                    historyBackfilled = true;
                }
                await activityRepository.RecordNewOverdueAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to update the recent activity feed.");
            }

            try
            {
                await Task.Delay(CheckInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }
}
