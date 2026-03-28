using MediatR;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Documents.Dtos;
using WAssis.Application.Modules.Documents.Interfaces;

namespace WAssis.Application.Modules.Documents.Commands;

public sealed class ReviewImportedDocumentCommandHandler(
    IImportedDocumentRepository repository,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<ReviewImportedDocumentCommand, ImportedDocumentDto?>
{
    public async Task<ImportedDocumentDto?> Handle(ReviewImportedDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(request.ImportedDocumentId, cancellationToken);
        if (document is null)
        {
            return null;
        }

        var reviewer = currentUserContext.UserId ?? "system-review";
        document.MarkReviewed(reviewer, request.Notes);
        await repository.SaveChangesAsync(cancellationToken);

        return ImportedDocumentMappings.ToDto(document);
    }
}
