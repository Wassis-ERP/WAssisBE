using WAssis.Domain.Modules.Documents.Entities;

namespace WAssis.Application.Modules.Documents.Interfaces;

public interface IImportedDocumentRepository
{
    Task AddAsync(ImportedDocument document, CancellationToken cancellationToken);
    Task<ImportedDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
