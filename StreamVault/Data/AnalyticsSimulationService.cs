using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StreamVault.Configuration;

namespace StreamVault.Data;

public class AnalyticsSimulationService(
    IServiceScopeFactory scopeFactory,
    IAnalyticsSimulator simulator,
    IOptions<AnalyticsOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var eventsPerSecond = Math.Max(1, options.Value.EventsPerSecond);
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1.0 / eventsPerSecond));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await SimulateNextEventAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task SimulateNextEventAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogueDbContext>();
        var analytics = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

        var contentIds = await db.Contents.Select(c => c.Id).ToListAsync(stoppingToken);
        if (contentIds.Count == 0)
        {
            return;
        }

        var play = simulator.NextPlay(contentIds);
        await analytics.RecordPlayAsync(play.ContentItemId, play.Country);

        if (play.Rating is { } positive)
        {
            await analytics.RecordRatingAsync(play.ContentItemId, positive);
        }
    }
}
