using WAssis.Application.Modules.Identity.Dtos;
using WAssis.Domain.Core.Messages;

namespace WAssis.Application.Modules.Identity.Interfaces;

public interface IIdentityAuthenticationService
{
    Task<Result<LoginResultDto>> AuthenticateAsync(string username, string password, CancellationToken cancellationToken);
}
