using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Modules.Documents.Commands;
using WAssis.Application.Modules.Documents.Dtos;
using WAssis.Application.Modules.Documents.Queries;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Services.Api.Modules.Documents.Contracts;
using WAssis.Services.Api.Modules.Documents.ViewModels;

namespace WAssis.Services.Api.Modules.Documents.Controllers;

[ApiController]
[Route("api/documents/proposals")]
[Authorize(Policy = AccessPolicies.BrokerageStaff)]
public sealed class ProposalDocumentsController(IMediator mediator) : ControllerBase
{
    [HttpPost("uploads")]
    [RequestSizeLimit(20_000_000)]
    [ProducesResponseType(typeof(ImportedDocumentViewModel), StatusCodes.Status201Created)]
    public async Task<IActionResult> Upload([FromForm] UploadProposalDocumentRequest request, CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest("Arquivo PDF é obrigatório.");
        }

        await using var memoryStream = new MemoryStream();
        await request.File.CopyToAsync(memoryStream, cancellationToken);

        var result = await mediator.Send(
            new UploadProposalDocumentCommand(
                request.CorrelationId ?? HttpContext.TraceIdentifier,
                request.File.FileName,
                request.File.ContentType,
                request.Source,
                memoryStream.ToArray()),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ToViewModel(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ImportedDocumentViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetImportedDocumentQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(ToViewModel(result));
    }

    [HttpPost("{id:guid}/review")]
    [ProducesResponseType(typeof(ImportedDocumentViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Review(Guid id, [FromBody] ReviewImportedDocumentRequest? request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ReviewImportedDocumentCommand(id, request?.Notes), cancellationToken);
        return result is null ? NotFound() : Ok(ToViewModel(result));
    }

    [HttpPost("{id:guid}/reprocess")]
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
            result.StoragePath,
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
