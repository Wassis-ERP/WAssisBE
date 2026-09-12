using FluentValidation;
using WAssis.Application.Modules.Customers.Commands;

namespace WAssis.Application.Modules.Customers.Validators;

public sealed class UpdateInsuredPersonCommandValidator : AbstractValidator<UpdateInsuredPersonCommand>
{
    public UpdateInsuredPersonCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OfficeBranchId).MaximumLength(64);
        RuleFor(x => x.PersonType).MaximumLength(8);
        RuleFor(x => x.Status).MaximumLength(32);
        RuleFor(x => x.DocumentNumber).MaximumLength(32);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.PhoneNumber).MaximumLength(32);
        RuleFor(x => x.SocialName).MaximumLength(200);
        RuleFor(x => x.IdentityDocument).MaximumLength(32);
        RuleFor(x => x.MunicipalRegistration).MaximumLength(64);
        RuleFor(x => x.EconomicActivity).MaximumLength(200);
        RuleFor(x => x.Profession).MaximumLength(120);
        RuleFor(x => x.MonthlyIncome).GreaterThanOrEqualTo(0).When(x => x.MonthlyIncome.HasValue);
        RuleFor(x => x.DriverLicenseNumber).MaximumLength(32);
        RuleFor(x => x.DriverLicenseCategory).MaximumLength(16);
        RuleFor(x => x.MobilePhoneNumber).MaximumLength(32);
        RuleFor(x => x.SecondaryPhoneNumber).MaximumLength(32);
        RuleFor(x => x.WhatsAppNumber).MaximumLength(32);
        RuleFor(x => x.Country).MaximumLength(100);
        RuleFor(x => x.ImportOrigin).MaximumLength(120);
    }
}
