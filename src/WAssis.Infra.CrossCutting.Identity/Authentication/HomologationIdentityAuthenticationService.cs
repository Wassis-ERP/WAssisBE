using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WAssis.Application.Modules.Identity.Dtos;
using WAssis.Application.Modules.Identity.Interfaces;
using WAssis.Domain.Core.Messages;
using WAssis.Infra.CrossCutting.Identity.Jwt;
using WAssis.Infra.CrossCutting.Identity.Models;

namespace WAssis.Infra.CrossCutting.Identity.Authentication;

public sealed class HomologationIdentityAuthenticationService(
    IOptions<HomologationAuthOptions> optionsAccessor,
    IOptions<JwtAccessOptions> jwtAccessor,
    IHostEnvironment environment) : IIdentityAuthenticationService
{
    private static readonly PasswordHasher<HomologationAuthUserOptions> Hasher = new();
    // Pay the same password-verification cost for unknown users; no real credential is stored here.
    private static readonly string DummyHash = Hasher.HashPassword(new(), Guid.NewGuid().ToString());

    public Task<Result<LoginResultDto>> AuthenticateAsync(string username, string password, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var options = optionsAccessor.Value;
        if (!environment.IsStaging() || !options.Enabled)
            return Failure("identity.login.disabled", "O mecanismo de autenticação não está habilitado neste ambiente.", ErrorType.Conflict);
        var validation = new HomologationAuthOptionsValidator(environment).Validate(null, options);
        if (!validation.Succeeded)
            return Failure("identity.login.misconfigured", "A configuração de autenticação está incompleta. Contate o administrador.", ErrorType.Conflict);

        var user = options.Users.SingleOrDefault(x => string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase));
        var verified = Hasher.VerifyHashedPassword(user ?? new(), user?.PasswordHash ?? DummyHash, password);
        if (user is null || verified == PasswordVerificationResult.Failed)
            return Failure("identity.login.invalid_credentials", "Usuário ou senha inválidos.", ErrorType.Unauthorized);

        var jwt = jwtAccessor.Value;
        if (!new JwtAccessOptionsValidator(environment).Validate(null, jwt).Succeeded)
            return Failure("identity.login.misconfigured", "A configuração de autenticação está incompleta. Contate o administrador.", ErrorType.Conflict);

        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(options.TokenExpirationMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId),
            new(ClaimTypes.NameIdentifier, user.UserId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimConstants.TenantId, user.TenantId),
            new(ClaimConstants.BrokerageId, user.BrokerageId),
            new(ClaimConstants.BranchId, user.BranchId),
            new(ClaimConstants.HasAllBranchesAccess, user.HasAllBranchesAccess.ToString()),
            new(ClaimConstants.UserType, user.UserType)
        };
        claims.AddRange(user.BranchIds.Distinct().Select(x => new Claim(ClaimConstants.BranchIds, x)));
        claims.AddRange(user.Roles.Distinct().Select(x => new Claim(ClaimTypes.Role, x)));
        if (user.SellerId is not null) claims.Add(new Claim(ClaimConstants.SellerId, user.SellerId));
        var token = new JwtSecurityToken(jwt.Issuer, jwt.Audience, claims, now, expires,
            new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)), SecurityAlgorithms.HmacSha256));
        return Task.FromResult(Result<LoginResultDto>.Success(new LoginResultDto(
            new JwtSecurityTokenHandler().WriteToken(token), expires, user.UserId, user.TenantId,
            user.BrokerageId, user.BranchId, user.BranchIds, user.HasAllBranchesAccess,
            user.SellerId, user.UserType, user.Roles)));
    }

    private static Task<Result<LoginResultDto>> Failure(string code, string message, ErrorType type)
        => Task.FromResult(Result<LoginResultDto>.Failure(new Error(code, message, type)));
}
