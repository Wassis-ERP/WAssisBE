using MediatR;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Modules.Financial.Commands;
using WAssis.Application.Modules.Financial.Queries;
using WAssis.Services.Api.Modules.Financial.Contracts;
using WAssis.Services.Api.Modules.Financial.ViewModels;

namespace WAssis.Services.Api.Modules.Financial.Controllers;

[ApiController]
[Route("api/financial")]
public sealed class CommissionReceiptsController(IMediator mediator) : ControllerBase
{
    [HttpPost("commission-receipts")]
    public async Task<IActionResult> RegisterReceipt([FromBody] RegisterCommissionReceiptRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RegisterCommissionReceiptCommand(
                request.CorrelationId ?? HttpContext.TraceIdentifier,
                request.InsuranceCompanyCode,
                request.ReceivedAmount,
                request.ExpectedAmount,
                request.SourceType,
                request.ImportedDocumentId),
            cancellationToken);

        return CreatedAtAction(nameof(GetReconciliation), new { id = result.Id }, ToViewModel(result));
    }

    [HttpGet("reconciliations/{id:guid}")]
    public async Task<IActionResult> GetReconciliation(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCommissionReconciliationQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(ToViewModel(result));
    }

    private static CommissionReconciliationViewModel ToViewModel(Application.Modules.Financial.Dtos.CommissionReconciliationDto result)
    {
        return new CommissionReconciliationViewModel(
            result.Id,
            result.CommissionReceiptId,
            result.ExpectedAmount,
            result.ReceivedAmount,
            result.DifferenceAmount,
            result.Status,
            result.CreatedAtUtc);
    }
}
