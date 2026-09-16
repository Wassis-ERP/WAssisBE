using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Modules.Financial.Commands;
using WAssis.Application.Modules.Financial.Queries;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Services.Api.Infrastructure;
using WAssis.Services.Api.Modules.Financial.Contracts;
using WAssis.Services.Api.Modules.Financial.ViewModels;

namespace WAssis.Services.Api.Modules.Financial.Controllers;

[ApiController]
[Route("api/financial")]
[Authorize(Policy = AccessPolicies.BrokerageStaff)]
public sealed class CommissionReceiptsController(IMediator mediator) : ControllerBase
{
    [HttpPost("commission-receipts")]
    [RequirePermission("financeiro", "create")]
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

    [HttpPost("statement-analyses")]
    [RequirePermission("financeiro", "create")]
    [DistributedOperationLimit("financial.statement-analysis", 10, 60)]
    [RequestSizeLimit(600_000)]
    public async Task<IActionResult> AnalyzeStatement([FromBody] AnalyzeCommissionStatementRequest request, CancellationToken cancellationToken)
    {
        if (request.RawText.Length > 500_000)
            return StatusCode(StatusCodes.Status413PayloadTooLarge, new ProblemDetails
            {
                Status = StatusCodes.Status413PayloadTooLarge,
                Title = "Conteúdo excede o limite permitido.",
            });
        var result = await mediator.Send(
            new AnalyzeCommissionStatementCommand(request.SourceType, request.RawText),
            cancellationToken);

        return Ok(new CommissionStatementAnalysisViewModel(
            result.SourceType,
            result.ParsedLines,
            result.LinesNeedingManualReview,
            result.Lines.Select(static line => new CommissionStatementLineViewModel(
                line.LineNumber,
                line.RawText,
                line.InsuranceCompanyCode,
                line.ProposalNumber,
                line.Amount,
                line.OccurredAtUtc,
                line.SuggestedReference,
                line.NeedsManualReview)).ToArray()));
    }

    [HttpGet("reconciliations/{id:guid}")]
    [RequirePermission("financeiro", "read")]
    public async Task<IActionResult> GetReconciliation(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCommissionReconciliationQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(ToViewModel(result));
    }

    [HttpPost("reconciliations/{id:guid}/settle")]
    [RequirePermission("financeiro", "update")]
    public async Task<IActionResult> Settle(Guid id, [FromBody] SettleCommissionReconciliationRequest? request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new SettleCommissionReconciliationCommand(id, request?.MatchedReference, request?.SettlementNotes),
            cancellationToken);

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
            result.MatchedReference,
            result.SettlementNotes,
            result.CreatedAtUtc,
            result.SettledAtUtc);
    }
}
