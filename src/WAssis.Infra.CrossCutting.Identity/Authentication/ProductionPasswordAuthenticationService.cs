using WAssis.Application.Modules.Identity.Dtos;
using WAssis.Application.Modules.Identity.Interfaces;
using WAssis.Domain.Core.Messages;

namespace WAssis.Infra.CrossCutting.Identity.Authentication;

public sealed class ProductionPasswordAuthenticationService : IIdentityAuthenticationService
{
    public Task<Result<LoginResultDto>> AuthenticateAsync(string username, string password, CancellationToken cancellationToken) =>
        Task.FromResult(Result<LoginResultDto>.Failure(new Error(
            "identity.login.external_required",
            "Use o provedor corporativo para entrar.",
            ErrorType.Conflict)));
}
