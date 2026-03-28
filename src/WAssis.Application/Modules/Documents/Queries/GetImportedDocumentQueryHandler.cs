using MediatR;
using WAssis.Application.Modules.Documents.Dtos;
using WAssis.Application.Modules.Documents.Interfaces;

namespace WAssis.Application.Modules.Documents.Queries;

public sealed class GetImportedDocumentQueryHandler(IImportedDocumentRepository repository)
    : IRequestHandler<GetImportedDocumentQuery, ImportedDocumentDto?>
{
    public async Task<ImportedDocumentDto?> Handle(GetImportedDocumentQuery request, CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(request.ImportedDocumentId, cancellationToken);
        return document is null ? null : ImportedDocumentMappings.ToDto(document);
    }
}
