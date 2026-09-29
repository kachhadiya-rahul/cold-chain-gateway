using System.Threading.Channels;
using Microsoft.Extensions.Caching.Memory;

namespace ColdChain.Gateway;

public class ReadingWorker(
    ChannelReader<Reading> queue,
    IMemoryCache cache,
    IServiceScopeFactory scopes,
    SemaphoreSlim dbLock,
    ILogger<ReadingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var reading in queue.ReadAllAsync(stoppingToken))
            await Check(reading, stoppingToken);
    }

    async Task Check(Reading reading, CancellationToken ct)
    {
        await dbLock.WaitAsync(ct);
        Tenant? tenant;
        try
        {
            tenant = cache.GetOrCreate(reading.TenantId, e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                using var scope = scopes.CreateScope();
                return scope.ServiceProvider.GetRequiredService<AppDbContext>()
                    .Tenants.Find(reading.TenantId);
            });
        }
        finally
        {
            dbLock.Release();
        }

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

        await dbLock.WaitAsync(ct);
        try
        {
            using var scope = scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Outbox.Add(new OutboxAlert
            {
                Id = Guid.NewGuid(),
                TenantId = reading.TenantId,
                DeviceId = reading.DeviceId,
                TemperatureC = reading.TemperatureC,
                MaxTemperatureC = tenant.MaxTemperatureC,
                RecordedAt = reading.RecordedAt
            });
            await db.SaveChangesAsync(ct);
        }
        finally
        {
            dbLock.Release();
        }
    }
}
