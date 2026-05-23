using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WAssis.Application.Modules.Identity.Dtos;
using WAssis.Application.Modules.Identity.Interfaces;
using WAssis.Domain.Core.Messages;
using WAssis.Infra.CrossCutting.Identity.Jwt;
using WAssis.Infra.CrossCutting.Identity.Models;

namespace WAssis.Infra.CrossCutting.Identity.Authentication;

public sealed class DevelopmentIdentityAuthenticationService(
    IOptions<DevelopmentAuthOptions> developmentAuthOptionsAccessor,
    IOptions<JwtAccessOptions> jwtOptionsAccessor,
    IHostEnvironment hostEnvironment) : IIdentityAuthenticationService
{
    private readonly DevelopmentAuthOptions _developmentAuthOptions = developmentAuthOptionsAccessor.Value;
    private readonly JwtAccessOptions _jwtOptions = jwtOptionsAccessor.Value;

    public Task<Result<LoginResultDto>> AuthenticateAsync(string username, string password, CancellationToken cancellationToken)
    {
        if ((!hostEnvironment.IsDevelopment() && !_developmentAuthOptions.AllowOutsideDevelopment) || !_developmentAuthOptions.Enabled)
        {
            return Task.FromResult(Result<LoginResultDto>.Failure(new Error(
                "identity.login.disabled",
                "Login local de desenvolvimento não está habilitado neste ambiente.",
                ErrorType.Conflict)));
        }

        var user = _developmentAuthOptions.Users.SingleOrDefault(user =>
            string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase));

        if (user is null || !string.Equals(user.Password, password, StringComparison.Ordinal))
        {
            return Task.FromResult(Result<LoginResultDto>.Failure(new Error(
                "identity.login.invalid_credentials",
                "Usuário ou senha inválidos.",
                ErrorType.Validation)));
        }

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_developmentAuthOptions.TokenExpirationMinutes);
        var token = BuildToken(user, expiresAtUtc);

        return Task.FromResult(Result<LoginResultDto>.Success(new LoginResultDto(
            token,
            expiresAtUtc,
            user.UserId,
            user.TenantId,
            user.BrokerageId,
            user.BranchId,
            user.BranchIds,
            user.HasAllBranchesAccess,
            user.SellerId,
            user.UserType,
            user.Roles)));
    }

    private string BuildToken(DevelopmentAuthUserOptions user, DateTime expiresAtUtc)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId),
            new(JwtRegisteredClaimNames.Sub, user.UserId),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(ClaimConstants.UserType, user.UserType)
        };

        if (!string.IsNullOrWhiteSpace(user.TenantId))
        {
            claims.Add(new Claim(ClaimConstants.TenantId, user.TenantId));
        }

        if (!string.IsNullOrWhiteSpace(user.BrokerageId))
        {
            claims.Add(new Claim(ClaimConstants.BrokerageId, user.BrokerageId));
        }

        if (!string.IsNullOrWhiteSpace(user.BranchId))
        {
            claims.Add(new Claim(ClaimConstants.BranchId, user.BranchId));
        }

        foreach (var branchId in user.BranchIds.Where(static x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(ClaimConstants.BranchIds, branchId.Trim()));
        }

        claims.Add(new Claim(ClaimConstants.HasAllBranchesAccess, user.HasAllBranchesAccess.ToString()));

        if (!string.IsNullOrWhiteSpace(user.SellerId))
        {
            claims.Add(new Claim(ClaimConstants.SellerId, user.SellerId));
        }

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
