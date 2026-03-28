using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using WAssis.Application.Modules.Quotes.Interfaces;

namespace WAssis.BackgroundTasks.Modules.Quotes;

public sealed class QuoteQuoteRequestDispatcherPlaceholder(
    ILogger<QuoteQuoteRequestDispatcherPlaceholder> logger,
    IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Quotes async dispatcher initialized. Processing pending requests through configured providers.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var quoteProcessingService = scope.ServiceProvider.GetRequiredService<IQuoteProcessingService>();
            var processedCount = await quoteProcessingService.ProcessPendingBatchAsync(25, stoppingToken);
            if (processedCount > 0)
            {
                logger.LogInformation(
                    "Quotes dispatcher processed {ProcessedCount} pending requests.",
                    processedCount);
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
