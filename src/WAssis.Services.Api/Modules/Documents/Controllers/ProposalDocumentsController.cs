using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Text;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Documents.Commands;
using WAssis.Application.Modules.Documents.Dtos;
using WAssis.Application.Modules.Documents.Interfaces;
using WAssis.Application.Modules.Documents.Queries;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Services.Api.Infrastructure;
using WAssis.Services.Api.Modules.Documents.Contracts;
using WAssis.Services.Api.Modules.Documents.ViewModels;

namespace WAssis.Services.Api.Modules.Documents.Controllers;

[ApiController]
[Route("api/documents/proposals")]
[Authorize(Policy = AccessPolicies.BrokerageStaff)]
public sealed class ProposalDocumentsController(
    IMediator mediator,
    ICurrentUserContext currentUser,
    IPrivateDocumentStorage privateStorage) : ControllerBase
{
    [HttpPost("uploads")]
    [RequirePermission("documentos", "create")]
    [DistributedOperationLimit("documents.upload", 5, 60)]
    [RequestSizeLimit(8_100_000)]
    [ProducesResponseType(typeof(ImportedDocumentViewModel), StatusCodes.Status201Created)]
    public async Task<IActionResult> Upload(CancellationToken cancellationToken)
    {
        if (!MediaTypeHeaderValue.TryParse(Request.ContentType, out var mediaType)
            || !string.Equals(mediaType.MediaType.Value, "multipart/form-data", StringComparison.OrdinalIgnoreCase))
            return StatusCode(StatusCodes.Status415UnsupportedMediaType);
        var boundary = HeaderUtilities.RemoveQuotes(mediaType.Boundary).Value;
        if (string.IsNullOrWhiteSpace(boundary) || boundary.Length > 128)
            return BadRequest(new ProblemDetails { Status = 400, Title = "Multipart inválido." });

        var documentId = Guid.NewGuid();
        var tenantId = currentUser.ResolveTenantIdOrPlatform();
        var branchId = currentUser.ResolveBranchIdForWrite()
            ?? throw new UnauthorizedAccessException("Filial ativa é obrigatória para armazenar documentos.");
        string? storageKey = null;
        string? fileName = null;
        string? contentType = null;
        string? correlationId = null;
        var source = "manual_upload";
        var accepted = false;
        try
        {
            var reader = new MultipartReader(boundary, Request.Body) { BodyLengthLimit = 8_050_000 };
            MultipartSection? section;
            while ((section = await reader.ReadNextSectionAsync(cancellationToken)) is not null)
            {
                if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var disposition)) continue;
                var name = HeaderUtilities.RemoveQuotes(disposition.Name).Value;
                var suppliedName = HeaderUtilities.RemoveQuotes(disposition.FileNameStar).Value
                    ?? HeaderUtilities.RemoveQuotes(disposition.FileName).Value;
                if (!string.IsNullOrWhiteSpace(suppliedName))
                {
                    if (storageKey is not null) throw new InvalidDataException("document_multiple_files");
                    fileName = Path.GetFileName(suppliedName);
                    contentType = section.ContentType;
                    if (!string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase)
                        || !string.Equals(Path.GetExtension(fileName), ".pdf", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException("document_format_unsupported");
                    storageKey = await privateStorage.StoreAsync(
                        documentId, tenantId, branchId, section.Body, cancellationToken);
                    continue;
                }

                var value = await ReadSmallFieldAsync(section.Body, cancellationToken);
                if (string.Equals(name, "correlationId", StringComparison.OrdinalIgnoreCase)) correlationId = value;
                else if (string.Equals(name, "source", StringComparison.OrdinalIgnoreCase)) source = value;
            }

            if (storageKey is null || string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(contentType))
                return BadRequest(new ProblemDetails { Status = 400, Title = "Arquivo PDF é obrigatório." });
            var result = await mediator.Send(new UploadProposalDocumentCommand(
                documentId,
                correlationId ?? HttpContext.TraceIdentifier,
                fileName,
                contentType,
                source,
                storageKey), cancellationToken);
            accepted = true;
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ToViewModel(result));
        }
        catch (InvalidDataException exception) when (exception.Message == "document_size_limit_exceeded")
        {
            return StatusCode(StatusCodes.Status413PayloadTooLarge, new ProblemDetails
            {
                Status = StatusCodes.Status413PayloadTooLarge,
                Title = "Arquivo excede o limite permitido.",
            });
        }
        catch (InvalidDataException)
        {
            return BadRequest(new ProblemDetails { Status = 400, Title = "Arquivo PDF inválido." });
        }
        finally
        {
            if (storageKey is not null && !accepted)
                await privateStorage.DeleteAsync(storageKey, CancellationToken.None);
        }
    }

    private static async Task<string> ReadSmallFieldAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, true, 256, leaveOpen: true);
        var buffer = new char[1025];
        var read = await reader.ReadBlockAsync(buffer.AsMemory(), cancellationToken);
        var extra = new char[1];
        if (read > 1024 || await reader.ReadAsync(extra.AsMemory(), cancellationToken) != 0)
            throw new InvalidDataException("document_field_limit_exceeded");
        return new string(buffer, 0, read).Trim();
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("documentos", "read")]
    [ProducesResponseType(typeof(ImportedDocumentViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetImportedDocumentQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(ToViewModel(result));
    }

    [HttpPost("{id:guid}/review")]
    [RequirePermission("documentos", "update")]
    [ProducesResponseType(typeof(ImportedDocumentViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Review(Guid id, [FromBody] ReviewImportedDocumentRequest? request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ReviewImportedDocumentCommand(id, request?.Notes), cancellationToken);
        return result is null ? NotFound() : Ok(ToViewModel(result));
    }

    [HttpPost("{id:guid}/reprocess")]
    [RequirePermission("documentos", "manage")]
    [ProducesResponseType(typeof(ImportedDocumentViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reprocess(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ReprocessImportedDocumentCommand(id), cancellationToken);
        return result is null ? NotFound() : Ok(ToViewModel(result));
    }

    private static ImportedDocumentViewModel ToViewModel(ImportedDocumentDto result)
    {
        return new ImportedDocumentViewModel(
            result.Id,
            result.CorrelationId,
            result.FileName,
            result.ContentType,
            result.Source,
            result.Status,
            result.DocumentType,
            result.InsuranceCompanyName,
            result.ProposalNumber,
            result.InsuredName,
            result.CoverageStartDateUtc,
            result.CoverageEndDateUtc,
            result.TotalPremiumAmount,
            result.CommissionAmount,
            result.ParsingConfidence,
            result.RequiresHumanReview,
            result.ReviewedAtUtc,
            result.ReviewedByUserId,
            result.ParsingNotes,
            result.CreatedAtUtc,
            result.LastProcessedAtUtc);
    }
}
