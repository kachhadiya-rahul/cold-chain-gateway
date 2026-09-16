using System.Threading.Channels;
using Microsoft.Extensions.Caching.Memory;

namespace ColdChain.Gateway;

public class ReadingWorker(
    ChannelReader<Reading> queue,
    IMemoryCache cache,
    IServiceScopeFactory scopes,
    ILogger<ReadingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var reading in queue.ReadAllAsync(stoppingToken))
            Check(reading);
    }

    void Check(Reading reading)
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

        if (reading.TemperatureC > tenant.MaxTemperatureC)
            logger.LogWarning("alert {Device} {Temp} > {Max}", reading.DeviceId, reading.TemperatureC, tenant.MaxTemperatureC);
        else
            logger.LogInformation("ok {Device} {Temp} <= {Max}", reading.DeviceId, reading.TemperatureC, tenant.MaxTemperatureC);
    }
}
