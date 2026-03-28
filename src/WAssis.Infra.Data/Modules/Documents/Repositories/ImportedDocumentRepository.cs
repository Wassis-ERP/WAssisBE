using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.Documents.Interfaces;
using WAssis.Domain.Modules.Documents.Entities;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Documents.Repositories;

public sealed class ImportedDocumentRepository(WAssisDbContext dbContext) : IImportedDocumentRepository
{
    public async Task AddAsync(ImportedDocument document, CancellationToken cancellationToken)
    {
        await dbContext.ImportedDocuments.AddAsync(document, cancellationToken);
    }

    public Task<ImportedDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.ImportedDocuments.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
