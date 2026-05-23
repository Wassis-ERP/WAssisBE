using FluentValidation;
using WAssis.Application.Modules.Customers.Commands;

namespace WAssis.Application.Modules.Customers.Validators;

public sealed class CreateInsuredPersonCommandValidator : AbstractValidator<CreateInsuredPersonCommand>
{
    public CreateInsuredPersonCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OfficeBranchId).MaximumLength(64);
        RuleFor(x => x.PersonType).MaximumLength(8);
        RuleFor(x => x.Status).MaximumLength(32);
        RuleFor(x => x.DocumentNumber).MaximumLength(32);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.PhoneNumber).MaximumLength(32);
    }
}
