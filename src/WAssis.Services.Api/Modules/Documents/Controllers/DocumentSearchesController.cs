using MediatR;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Modules.Documents.Commands;
using WAssis.Application.Modules.Documents.Queries;
using WAssis.Services.Api.Modules.Documents.Contracts;
using WAssis.Services.Api.Modules.Documents.ViewModels;

namespace WAssis.Services.Api.Modules.Documents.Controllers;

[ApiController]
[Route("api/documents/searches")]
public sealed class DocumentSearchesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDocumentSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateDocumentSearchCommand(
                request.CorrelationId ?? HttpContext.TraceIdentifier,
                request.InsuranceCompanyCode,
                request.SearchType),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new DocumentSearchViewModel(
            result.Id,
            result.CorrelationId,
            result.InsuranceCompanyCode,
            result.SearchType,
            result.Status,
            result.CreatedAtUtc));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDocumentSearchQuery(id), cancellationToken);
        return result is null
            ? NotFound()
            : Ok(new DocumentSearchViewModel(
                result.Id,
                result.CorrelationId,
                result.InsuranceCompanyCode,
                result.SearchType,
                result.Status,
                result.CreatedAtUtc));
    }
}
