using MediatR;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Documents.Dtos;
using WAssis.Application.Modules.Documents.Interfaces;
using WAssis.Domain.Modules.Documents.Entities;

namespace WAssis.Application.Modules.Documents.Commands;

public sealed class UploadProposalDocumentCommandHandler(
    ICurrentUserContext currentUserContext,
    IImportedDocumentRepository repository,
    IPdfTextExtractor pdfTextExtractor,
    IOcrTextExtractor ocrTextExtractor,
    IProposalDocumentParser proposalDocumentParser)
    : IRequestHandler<UploadProposalDocumentCommand, ImportedDocumentDto>
{
    public async Task<ImportedDocumentDto> Handle(UploadProposalDocumentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserContext.ResolveTenantIdOrPlatform();
        var storagePath = $"tenants/{tenantId}/documents/proposals/{Guid.NewGuid():N}/{request.FileName}";
        var document = ImportedDocument.Create(
            tenantId,
            request.CorrelationId,
            request.FileName,
            request.ContentType,
            request.Source,
            storagePath);

        document.MarkAsProcessing();

        await repository.AddAsync(document, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        try
        {
            var extractedText = await pdfTextExtractor.ExtractTextAsync(request.Content, cancellationToken);
            var parsingOrigin = "pdf_text_layer";

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                var ocrText = await ocrTextExtractor.ExtractTextAsync(request.Content, cancellationToken);
                if (!string.IsNullOrWhiteSpace(ocrText))
                {
                    extractedText = ocrText;
                    parsingOrigin = "ocr_fallback";
                }
            }

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                document.MarkAsFailed("PDF sem camada textual utilizável e OCR não retornou conteúdo legível.");
                await repository.SaveChangesAsync(cancellationToken);
                return ImportedDocumentMappings.ToDto(document);
            }

            var parsingResult = proposalDocumentParser.Parse(extractedText);
            var parsingNotes = string.IsNullOrWhiteSpace(parsingResult.ParsingNotes)
                ? $"Parser executado usando {parsingOrigin}."
                : $"{parsingResult.ParsingNotes} Origem do texto: {parsingOrigin}.";

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
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (InvalidDataException ex)
        {
            document.MarkAsFailed($"PDF parsing failed: {ex.Message}");
        }
        catch (IOException ex)
        {
            document.MarkAsFailed($"PDF parsing failed: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            document.MarkAsFailed($"PDF parsing failed: {ex.Message}");
        }
        catch (FormatException ex)
        {
            document.MarkAsFailed($"PDF parsing failed: {ex.Message}");
        }

        await repository.SaveChangesAsync(cancellationToken);

        return ImportedDocumentMappings.ToDto(document);
    }
}
