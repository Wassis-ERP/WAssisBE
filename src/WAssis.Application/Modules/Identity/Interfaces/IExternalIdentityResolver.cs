using WAssis.Application.Modules.Identity.Dtos;

namespace WAssis.Application.Modules.Identity.Interfaces;

public interface IExternalIdentityResolver
{
    Task<ExternalIdentityDto?> ResolveAsync(string issuer, string subject, CancellationToken cancellationToken);
}
