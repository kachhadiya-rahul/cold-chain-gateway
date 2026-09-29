using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ColdChain.Gateway;

public class OutboxPublisher(
    IServiceScopeFactory scopes,
    IHubContext<AlertsHub> hubs,
    SemaphoreSlim dbLock,
    ILogger<OutboxPublisher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var tick = new PeriodicTimer(TimeSpan.FromMilliseconds(250));
        await Flush(stoppingToken);
        while (await tick.WaitForNextTickAsync(stoppingToken))
            await Flush(stoppingToken);
    }

    async Task Flush(CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbLock.WaitAsync(ct);
        List<OutboxAlert> due;
        try
        {
            due = await db.Outbox
                .Where(a => a.PublishedAt == null)
                .OrderBy(a => a.RecordedAt)
                .Take(32)
                .ToListAsync(ct);
        }
        finally
        {
            dbLock.Release();
        }

        foreach (var alert in due)
        {
            try
            {
                await hubs.Clients.Group(alert.TenantId).SendAsync("alert", new
                {
                    alert.DeviceId,
                    alert.TemperatureC,
                    alert.MaxTemperatureC,
                    alert.RecordedAt
                }, ct);
                alert.PublishedAt = DateTimeOffset.UtcNow;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "publish failed {Device}", alert.DeviceId);
            }
        }

        await dbLock.WaitAsync(ct);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        finally
        {
            dbLock.Release();
        }
    }
}
