using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WAssis.BackgroundTasks;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Background heartbeat running at {TimestampUtc}", DateTime.UtcNow);
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
