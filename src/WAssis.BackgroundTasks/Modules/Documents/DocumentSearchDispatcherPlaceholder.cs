using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using WAssis.Application.Modules.Documents.Interfaces;

namespace WAssis.BackgroundTasks.Modules.Documents;

public sealed class DocumentSearchDispatcherPlaceholder(
    ILogger<DocumentSearchDispatcherPlaceholder> logger,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Documents dispatcher placeholder initialized for automatic search and parsing using repository {RepositoryType}.",
            nameof(IDocumentSearchRepository));

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            _ = scope.ServiceProvider.GetRequiredService<IDocumentSearchRepository>();
            logger.LogDebug("Documents dispatcher heartbeat at {TimestampUtc}", DateTime.UtcNow);
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
