using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WAssis.Application.Modules.Documents.Interfaces;

namespace WAssis.BackgroundTasks.Modules.Documents;

public sealed class DocumentProcessingDispatcher(
    ILogger<DocumentProcessingDispatcher> logger,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Durable isolated document parser initialized.");
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IDocumentProcessingService>();
            var processed = await service.ProcessPendingBatchAsync(10, stoppingToken);
            if (processed > 0)
                logger.LogInformation("Document worker processed {ProcessedCount} queued documents.", processed);
            await Task.Delay(processed > 0 ? TimeSpan.FromSeconds(1) : TimeSpan.FromSeconds(15), stoppingToken);
        }
    }
}
