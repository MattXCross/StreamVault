using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StreamVault.Configuration;

namespace StreamVault.Data;

public class PlayEventPurgeService(
    IServiceScopeFactory scopeFactory,
    IOptions<AnalyticsOptions> options) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        try
        {
            do
            {
                await PurgeExpiredAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task PurgeExpiredAsync(CancellationToken stoppingToken)
    {
        var cutoff = DateTime.UtcNow.AddDays(-options.Value.PlayEventRetentionDays);

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogueDbContext>();

        await db.PlayEvents
            .Where(e => e.PlayedAt < cutoff)
            .ExecuteDeleteAsync(stoppingToken);
    }
}
