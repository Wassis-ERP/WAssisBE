namespace WAssis.Application.Modules.Documents.Interfaces;

public interface IDocumentProcessingService
{
    Task<int> ProcessPendingBatchAsync(int batchSize, CancellationToken cancellationToken);
}
