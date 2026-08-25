using MediatR;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Domain.Modules.Quotes.Entities;

namespace WAssis.Application.Modules.Quotes.Commands;

public sealed class CreateQuoteRequestCommandHandler(
    IQuoteRequestRepository repository,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<CreateQuoteRequestCommand, QuoteRequestDto>
{
    public async Task<QuoteRequestDto> Handle(CreateQuoteRequestCommand request, CancellationToken cancellationToken)
    {
        var existingQuoteRequest = await repository.GetByCorrelationIdAsync(request.CorrelationId, cancellationToken);
        if (existingQuoteRequest is not null)
        {
            return QuoteRequestMappings.ToDto(existingQuoteRequest);
        }

        var quoteRequest = QuoteRequest.Create(
            currentUserContext.ResolveTenantIdOrPlatform(),
            request.CorrelationId,
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
            currentUserContext.ResolveBranchIdForWrite(request.OfficeBranchId),
            request.OpportunityId,
            request.InsuranceBranchId,
            request.InsuredPersonId,
            request.CalculationType,
            request.CalculationOrigin,
            request.VersionLabel);

        await repository.AddAsync(quoteRequest, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return QuoteRequestMappings.ToDto(quoteRequest);
    }
}
