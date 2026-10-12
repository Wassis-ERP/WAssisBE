using WAssis.Application.Modules.Identity.Dtos;

namespace WAssis.Application.Modules.Identity.Interfaces;

public interface IInternalAccessTokenIssuer
{
    LoginResultDto Issue(ExternalIdentityDto identity);
}
