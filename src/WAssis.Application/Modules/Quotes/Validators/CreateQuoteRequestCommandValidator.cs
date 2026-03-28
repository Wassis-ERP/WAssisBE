using FluentValidation;
using WAssis.Application.Modules.Quotes.Commands;

namespace WAssis.Application.Modules.Quotes.Validators;

public sealed class CreateQuoteRequestCommandValidator : AbstractValidator<CreateQuoteRequestCommand>
{
    public CreateQuoteRequestCommandValidator()
    {
        RuleFor(x => x.CorrelationId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DocumentNumber).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Email).MaximumLength(200);
        RuleFor(x => x.PhoneNumber).MaximumLength(32);
        RuleFor(x => x.PostalCode).MaximumLength(16);
        RuleFor(x => x.CustomerSurname).MaximumLength(120);
        RuleFor(x => x.CustomerGender).MaximumLength(1);
        RuleFor(x => x.VehiclePlate).MaximumLength(16);
        RuleFor(x => x.VehicleBrand).MaximumLength(100);
        RuleFor(x => x.VehicleModel).MaximumLength(100);
        RuleFor(x => x.VehicleFipeCode).MaximumLength(32);
        RuleFor(x => x.VehicleModelYear).InclusiveBetween(1900, 2100);
        RuleFor(x => x.PreviousBonus).MaximumLength(4);
        RuleFor(x => x.BrokerCommissionPercentage)
            .InclusiveBetween(10, 25)
            .When(x => x.BrokerCommissionPercentage.HasValue);
        RuleFor(x => x.CustomerGender)
            .Must(static gender => string.IsNullOrWhiteSpace(gender) || gender is "M" or "F")
            .WithMessage("CustomerGender must be 'M' or 'F'.");
    }
}
