using MediatR;
using WAssis.Application.Modules.Identity.Dtos;
using WAssis.Application.Modules.Identity.Interfaces;
using WAssis.Domain.Core.Messages;

namespace WAssis.Application.Modules.Identity.Commands;

public sealed class LoginCommandHandler(IIdentityAuthenticationService authenticationService)
    : IRequestHandler<LoginCommand, Result<LoginResultDto>>
{
    public Task<Result<LoginResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return authenticationService.AuthenticateAsync(request.Username, request.Password, cancellationToken);
    }
}
