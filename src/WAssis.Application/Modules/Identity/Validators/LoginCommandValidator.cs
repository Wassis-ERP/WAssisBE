using FluentValidation;
using WAssis.Application.Modules.Identity.Commands;

namespace WAssis.Application.Modules.Identity.Validators;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(200);
    }
}
