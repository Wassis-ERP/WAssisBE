namespace WAssis.Application.Modules.Quotes.Interfaces;

public interface IQuoteProcessingService
{
    Task<int> ProcessPendingBatchAsync(int batchSize, CancellationToken cancellationToken);
}
