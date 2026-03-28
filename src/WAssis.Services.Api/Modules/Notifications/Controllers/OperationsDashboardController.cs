using MediatR;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Modules.Notifications.Queries;
using WAssis.Services.Api.Modules.Notifications.ViewModels;

namespace WAssis.Services.Api.Modules.Notifications.Controllers;

[ApiController]
[Route("api/operations/dashboard")]
public sealed class OperationsDashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOperationsDashboardQuery(), cancellationToken);
        return Ok(new OperationsDashboardViewModel(
            result.PendingQuotes,
            result.PendingDocumentSearches,
            result.FailedImportedDocuments,
            result.DivergentReconciliations,
            result.PoliciesNeedingReview,
            result.WaitingWhatsAppHandoffs,
            result.AuditEntriesLast24Hours));
    }
}
