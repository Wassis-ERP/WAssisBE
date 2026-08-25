using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Modules.Quotes.Commands;
using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Modules.Quotes.Queries;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Services.Api.Modules.Quotes.Contracts;
using WAssis.Services.Api.Modules.Quotes.ViewModels;

namespace WAssis.Services.Api.Modules.Quotes.Controllers;

[ApiController]
[Route("api/quotes/requests")]
[Authorize(Policy = AccessPolicies.BrokerageStaff)]
public sealed class QuoteRequestsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<QuoteResponseViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] Guid? opportunityId,
        [FromQuery] string? officeBranchId,
        CancellationToken cancellationToken)
    {
        var response = await mediator.Send(
            new ListQuoteCalculationsQuery(opportunityId, officeBranchId),
            cancellationToken);

        return Ok(response.Select(ToViewModel));
    }

    [HttpPost]
    [ProducesResponseType(typeof(QuoteResponseViewModel), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateQuoteRequestRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateQuoteRequestCommand(
            request.CorrelationId ?? HttpContext.TraceIdentifier,
            request.CustomerName,
            request.DocumentNumber,
            request.Email,
            request.PhoneNumber,
            request.PostalCode,
            request.CustomerSurname,
            request.CustomerGender,
            request.CustomerMaritalStatusCode,
            request.CustomerBirthDateUtc,
            request.DriverLicenseYears,
            request.DriverLicenseNumber,
            request.InsuredDriverRelationshipCode,
            request.VehicleChassisNumber,
            request.VehiclePlate,
            request.VehicleBrand,
            request.VehicleModel,
            request.VehicleFipeCode,
            request.VehicleManufactureYear,
            request.VehicleModelYear,
            request.VehicleIsZeroKm,
            request.VehicleHasTracker,
            request.VehicleHasAntiTheft,
            request.VehicleIsFinanced,
            request.VehicleIsArmored,
            request.VehicleFuelTypeCode,
            request.VehicleOvernightPostalCode,
            request.VehicleHasKitGas,
            request.HasDriverUnder24,
            request.IsCurrentlyInsured,
            request.PreviousBonus,
            request.BrokerCommissionPercentage,
            request.RenewalInsurerCode,
            request.OfficeBranchId,
            request.OpportunityId,
            request.InsuranceBranchId,
            request.InsuredPersonId,
            request.CalculationType,
            request.CalculationOrigin,
            request.VersionLabel);

        var response = await mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, ToViewModel(response));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(QuoteResponseViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetQuoteStatusQuery(id), cancellationToken);
        return response is null ? NotFound() : Ok(ToViewModel(response));
    }

    [HttpGet("{id:guid}/results")]
    [ProducesResponseType(typeof(QuoteResultsViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResults(Guid id, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetQuoteStatusQuery(id), cancellationToken);
        if (response is null)
        {
            return NotFound();
        }

        return Ok(new QuoteResultsViewModel(
            response.Id,
            response.Status.ToString(),
            response.Options.Select(static option => new QuoteOptionViewModel(
                option.Id,
                option.InsuranceCompanyCode,
                option.InsuranceCompanyName,
                option.ProductCode,
                option.ProductName,
                option.Status,
                option.PremiumAmount,
                option.CommissionAmount,
                option.ExternalReference,
                option.Coverages.Select(static coverage => new CoverageSnapshotViewModel(
                    coverage.Code,
                    coverage.Name,
                    coverage.InsuredAmount,
                    coverage.DeductibleAmount)).ToArray(),
                option.Installments.Select(static installment => new InstallmentSnapshotViewModel(
                    installment.Number,
                    installment.Amount,
                    installment.TotalAmount)).ToArray(),
                option.Messages.Select(static message => new QuoteStatusMessageViewModel(
                    message.Code,
                    message.Description)).ToArray())).ToArray()));
    }

    private static QuoteResponseViewModel ToViewModel(QuoteRequestDto response)
    {
        return new QuoteResponseViewModel(
            response.Id,
            response.OfficeBranchId,
            response.OpportunityId,
            response.InsuranceBranchId,
            response.InsuredPersonId,
            response.CalculationType,
            response.CalculationOrigin,
            response.VersionLabel,
            response.CorrelationId,
            response.CustomerName,
            response.DocumentNumber,
            response.Email,
            response.PhoneNumber,
            response.PostalCode,
            response.CustomerSurname,
            response.CustomerGender,
            response.CustomerMaritalStatusCode,
            response.CustomerBirthDateUtc,
            response.DriverLicenseYears,
            response.DriverLicenseNumber,
            response.InsuredDriverRelationshipCode,
            response.VehicleChassisNumber,
            response.VehiclePlate,
            response.VehicleBrand,
            response.VehicleModel,
            response.VehicleFipeCode,
            response.VehicleManufactureYear,
            response.VehicleModelYear,
            response.VehicleIsZeroKm,
            response.VehicleHasTracker,
            response.VehicleHasAntiTheft,
            response.VehicleIsFinanced,
            response.VehicleIsArmored,
            response.VehicleFuelTypeCode,
            response.VehicleOvernightPostalCode,
            response.VehicleHasKitGas,
            response.HasDriverUnder24,
            response.IsCurrentlyInsured,
            response.PreviousBonus,
            response.BrokerCommissionPercentage,
            response.RenewalInsurerCode,
            response.Status,
            response.ShareToken,
            response.CreatedAtUtc,
            response.UpdatedAtUtc,
            response.Options.Select(static option => new QuoteOptionViewModel(
                option.Id,
                option.InsuranceCompanyCode,
                option.InsuranceCompanyName,
                option.ProductCode,
                option.ProductName,
                option.Status,
                option.PremiumAmount,
                option.CommissionAmount,
                option.ExternalReference,
                option.Coverages.Select(static coverage => new CoverageSnapshotViewModel(
                    coverage.Code,
                    coverage.Name,
                    coverage.InsuredAmount,
                    coverage.DeductibleAmount)).ToArray(),
                option.Installments.Select(static installment => new InstallmentSnapshotViewModel(
                    installment.Number,
                    installment.Amount,
                    installment.TotalAmount)).ToArray(),
                option.Messages.Select(static message => new QuoteStatusMessageViewModel(
                    message.Code,
                    message.Description)).ToArray())).ToArray());
    }
}
