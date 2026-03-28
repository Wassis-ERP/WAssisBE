using WAssis.Domain.Modules.Documents.Entities;

namespace WAssis.Application.Modules.Documents.Interfaces;

public interface IDocumentSearchRepository
{
    Task AddAsync(DocumentSearch search, CancellationToken cancellationToken);
    Task<DocumentSearch?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<DocumentSearch?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
