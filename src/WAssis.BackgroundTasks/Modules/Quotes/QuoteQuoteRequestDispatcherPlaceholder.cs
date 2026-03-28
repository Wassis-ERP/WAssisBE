using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WAssis.BackgroundTasks.Modules.Quotes;

public sealed class QuoteQuoteRequestDispatcherPlaceholder(ILogger<QuoteQuoteRequestDispatcherPlaceholder> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Quotes async dispatcher placeholder initialized. Future work: outbox polling, queue dispatch and provider orchestration.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
