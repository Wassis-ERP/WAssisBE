using MediatR;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Documents.Dtos;
using WAssis.Application.Modules.Documents.Interfaces;
using WAssis.Domain.Modules.Documents.Entities;

namespace WAssis.Application.Modules.Documents.Commands;

public sealed class UploadProposalDocumentCommandHandler(
    ICurrentUserContext currentUserContext,
    IImportedDocumentRepository repository)
    : IRequestHandler<UploadProposalDocumentCommand, ImportedDocumentDto>
{
    public async Task<ImportedDocumentDto> Handle(UploadProposalDocumentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserContext.ResolveTenantIdOrPlatform();
        var branchId = currentUserContext.ResolveBranchIdForWrite()
            ?? throw new UnauthorizedAccessException("Filial ativa é obrigatória para armazenar documentos.");
        var document = ImportedDocument.CreateStored(
            request.DocumentId,
            tenantId,
            request.CorrelationId,
            request.FileName,
            request.ContentType,
            request.Source,
            request.StorageKey,
            branchId);

        await repository.AddAsync(document, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return ImportedDocumentMappings.ToDto(document);
    }
}
