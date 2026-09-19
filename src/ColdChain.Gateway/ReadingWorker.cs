using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace ColdChain.Gateway;

public class ReadingWorker(
    ChannelReader<Reading> queue,
    IMemoryCache cache,
    IServiceScopeFactory scopes,
    IHubContext<AlertsHub> hubs,
    ILogger<ReadingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var reading in queue.ReadAllAsync(stoppingToken))
            await Check(reading, stoppingToken);
    }

    async Task Check(Reading reading, CancellationToken ct)
    {
        var tenant = cache.GetOrCreate(reading.TenantId, e =>
        {
            e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            using var scope = scopes.CreateScope();
            return scope.ServiceProvider.GetRequiredService<AppDbContext>()
                .Tenants.Find(reading.TenantId);
        });

        if (tenant is null)
        {
            logger.LogWarning("unknown tenant {Tenant}", reading.TenantId);
            return;
        }

        if (reading.TemperatureC <= tenant.MaxTemperatureC)
        {
            logger.LogInformation("ok {Device} {Temp} <= {Max}", reading.DeviceId, reading.TemperatureC, tenant.MaxTemperatureC);
            return;
        }

        logger.LogWarning("alert {Device} {Temp} > {Max}", reading.DeviceId, reading.TemperatureC, tenant.MaxTemperatureC);
        await hubs.Clients.Group(reading.TenantId).SendAsync("alert", new
        {
            reading.DeviceId,
            reading.TemperatureC,
            tenant.MaxTemperatureC,
            reading.RecordedAt
        }, ct);
    }
}
