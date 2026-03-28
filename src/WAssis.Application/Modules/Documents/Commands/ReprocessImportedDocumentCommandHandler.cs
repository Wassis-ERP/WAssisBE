using MediatR;
using WAssis.Application.Modules.Documents.Dtos;
using WAssis.Application.Modules.Documents.Interfaces;
using WAssis.Domain.Modules.Documents.Enums;

namespace WAssis.Application.Modules.Documents.Commands;

public sealed class ReprocessImportedDocumentCommandHandler(
    IImportedDocumentRepository repository,
    IProposalDocumentParser proposalDocumentParser)
    : IRequestHandler<ReprocessImportedDocumentCommand, ImportedDocumentDto?>
{
    public async Task<ImportedDocumentDto?> Handle(ReprocessImportedDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(request.ImportedDocumentId, cancellationToken);
        if (document is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(document.ExtractedText))
        {
            document.MarkAsFailed("Documento sem texto extraido para reprocessamento.");
            await repository.SaveChangesAsync(cancellationToken);
            return ImportedDocumentMappings.ToDto(document);
        }

        document.MarkAsProcessing();

        var parsingResult = proposalDocumentParser.Parse(document.ExtractedText);
        var parsingNotes = string.IsNullOrWhiteSpace(parsingResult.ParsingNotes)
            ? "Documento reprocessado a partir do texto extraido ja existente."
            : $"{parsingResult.ParsingNotes} Documento reprocessado a partir do texto extraido ja existente.";

        document.MarkAsParsed(
            parsingResult.DocumentType,
            parsingResult.ExtractedText,
            parsingResult.InsuranceCompanyName,
            parsingResult.ProposalNumber,
            parsingResult.InsuredName,
            parsingResult.CoverageStartDateUtc,
            parsingResult.CoverageEndDateUtc,
            parsingResult.TotalPremiumAmount,
            parsingResult.CommissionAmount,
            parsingResult.ParsingConfidence,
            parsingResult.RequiresHumanReview,
            parsingNotes);

        if (document.Status == ImportedDocumentStatus.Parsed)
        {
            await repository.SaveChangesAsync(cancellationToken);
        }

        return ImportedDocumentMappings.ToDto(document);
    }
}
