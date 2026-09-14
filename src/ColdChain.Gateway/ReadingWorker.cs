using System.Threading.Channels;

namespace ColdChain.Gateway;

public class ReadingWorker(ChannelReader<Reading> queue, ILogger<ReadingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var reading in queue.ReadAllAsync(stoppingToken))
            logger.LogInformation("{Device} {Temp}", reading.DeviceId, reading.TemperatureC);
    }
}
