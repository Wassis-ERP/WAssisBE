using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Modules.Quotes.Queries;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Services.Api.Modules.Quotes.ViewModels;

namespace WAssis.Services.Api.Modules.Quotes.Controllers;

[ApiController]
[Route("api/quotes/providers")]
[Authorize(Policy = AccessPolicies.BrokerageStaff)]
public sealed class QuoteProvidersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<QuoteProviderDescriptorViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetQuoteProvidersQuery(), cancellationToken);

        var viewModel = response
            .Select(static provider => new QuoteProviderDescriptorViewModel(
                provider.ProviderCode,
                provider.ProviderName,
                provider.IsEnabled,
                provider.IsReady,
                provider.AuthMode,
                provider.ProductLine,
                provider.OfficialDocumentationUrl,
                provider.Requirements
                    .Select(static requirement => new QuoteProviderRequirementViewModel(
                        requirement.Code,
                        requirement.Description,
                        requirement.IsConfigured))
                    .ToArray(),
                provider.Messages
                    .Select(static message => new QuoteStatusMessageViewModel(
                        message.Code,
                        message.Description))
                    .ToArray()))
            .ToArray();

        return Ok(viewModel);
    }
}
