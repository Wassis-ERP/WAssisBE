using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WAssis.Application.Modules.Identity.Dtos;
using WAssis.Application.Modules.Identity.Interfaces;
using WAssis.Infra.CrossCutting.Identity.Jwt;
using WAssis.Infra.CrossCutting.Identity.Models;

namespace WAssis.Infra.CrossCutting.Identity.Authentication;

public sealed class InternalAccessTokenIssuer(
    IOptions<JwtAccessOptions> jwtOptionsAccessor,
    IOptions<Auth0Options> auth0OptionsAccessor) : IInternalAccessTokenIssuer
{
    public LoginResultDto Issue(ExternalIdentityDto identity)
    {
        var jwtOptions = jwtOptionsAccessor.Value;
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(auth0OptionsAccessor.Value.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, identity.UserId),
            new(JwtRegisteredClaimNames.Sub, identity.UserId),
            new(ClaimConstants.TenantId, identity.TenantId),
            new(ClaimConstants.BrokerageId, identity.BrokerageId),
            new(ClaimConstants.BranchId, identity.BranchId),
            new(ClaimConstants.UserType, identity.UserType),
            new(ClaimConstants.HasAllBranchesAccess, bool.FalseString),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        claims.AddRange(identity.BranchIds.Select(branch => new Claim(ClaimConstants.BranchIds, branch)));
        claims.AddRange(identity.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        if (identity.SellerId is not null) claims.Add(new Claim(ClaimConstants.SellerId, identity.SellerId));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            jwtOptions.Issuer,
            jwtOptions.Audience,
            claims,
            DateTime.UtcNow,
            expiresAtUtc,
            credentials);
        var encoded = new JwtSecurityTokenHandler().WriteToken(token);
        return new LoginResultDto(
            encoded, expiresAtUtc, identity.UserId, identity.TenantId, identity.BrokerageId,
            identity.BranchId, identity.BranchIds, false, identity.SellerId, identity.UserType, identity.Roles);
    }
}
