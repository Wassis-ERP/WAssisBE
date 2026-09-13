using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WAssis.Application.Modules.Identity.Interfaces;
using WAssis.Infra.CrossCutting.Identity.Authentication;
using WAssis.Infra.CrossCutting.Identity.Extensions;
using WAssis.Infra.CrossCutting.Identity.Jwt;
using WAssis.Infra.CrossCutting.Identity.Models;

namespace WAssis.Tests.Security;

public sealed class HomologationAuthenticationTests
{
    private const string TestPassword = "test-only-password-not-a-deployed-credential";
    private static readonly string TestHash = new PasswordHasher<HomologationAuthUserOptions>().HashPassword(new(), TestPassword);
    private static JwtAccessOptions Jwt() => new()
    {
        SigningKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48)),
        RequireHttpsMetadata = true
    };

    private static HomologationAuthOptions ValidOptions() => new()
    {
        Enabled = true,
        Users = [new()
        {
            Username = "hml-test@example.invalid", PasswordHash = TestHash,
            UserId = "11111111-1111-1111-1111-111111111111",
            TenantId = "22222222-2222-2222-2222-222222222222",
            BrokerageId = "33333333-3333-3333-3333-333333333333",
            BranchId = "44444444-4444-4444-4444-444444444444",
            BranchIds = ["44444444-4444-4444-4444-444444444444"],
            UserType = UserTypes.BrokerageStaff, Roles = [AppRoles.BrokerageAdmin]
        }]
    };

    [Theory]
    [InlineData("Production", true)]
    [InlineData("Development", true)]
    [InlineData("HML", true)]
    [InlineData("Staging", false)]
    public async Task Login_IsFailClosed(string environment, bool enabled)
    {
        var options = ValidOptions();
        options.Enabled = enabled;
        var service = new HomologationIdentityAuthenticationService(Options.Create(options), Options.Create(Jwt()), new TestEnvironment(environment));
        var result = await service.AuthenticateAsync(options.Users[0].Username, TestPassword, default);
        Assert.False(result.IsSuccess);
        Assert.Equal("identity.login.disabled", result.Error?.Code);
    }

    [Fact]
    public async Task Login_IssuesValidShortTokenWithExactScope()
    {
        var options = ValidOptions();
        var jwt = Jwt();
        var service = new HomologationIdentityAuthenticationService(Options.Create(options), Options.Create(jwt), new TestEnvironment("Staging"));
        var result = await service.AuthenticateAsync(options.Users[0].Username.ToUpperInvariant(), TestPassword, default);
        Assert.True(result.IsSuccess);
        var login = Assert.IsType<WAssis.Application.Modules.Identity.Dtos.LoginResultDto>(result.Value);
        var principal = new JwtSecurityTokenHandler().ValidateToken(login.AccessToken, new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer, ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidateIssuerSigningKey = true, ValidateLifetime = true, ClockSkew = TimeSpan.Zero
        }, out var validated);
        Assert.Equal(options.Users[0].TenantId, principal.FindFirst(ClaimConstants.TenantId)?.Value);
        Assert.Equal(options.Users[0].BrokerageId, principal.FindFirst(ClaimConstants.BrokerageId)?.Value);
        Assert.Equal(options.Users[0].BranchId, principal.FindFirst(ClaimConstants.BranchId)?.Value);
        Assert.Equal(options.Users[0].BranchIds, principal.FindAll(ClaimConstants.BranchIds).Select(x => x.Value));
        Assert.Equal("False", principal.FindFirst(ClaimConstants.HasAllBranchesAccess)?.Value);
        Assert.Equal(options.Users[0].UserId, principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.True(principal.IsInRole(AppRoles.BrokerageAdmin));
        Assert.False(principal.IsInRole(AppRoles.PlatformAdmin));
        Assert.InRange((validated.ValidTo - DateTime.UtcNow).TotalMinutes, 14, 15);
    }

    [Theory]
    [InlineData("hml-test@example.invalid", "wrong")]
    [InlineData("unknown@example.invalid", TestPassword)]
    public async Task Login_RejectsUnknownUserAndWrongPassword(string username, string password)
    {
        var service = new HomologationIdentityAuthenticationService(Options.Create(ValidOptions()), Options.Create(Jwt()), new TestEnvironment("Staging"));
        var result = await service.AuthenticateAsync(username, password, default);
        Assert.Equal("identity.login.invalid_credentials", result.Error?.Code);
    }

    [Theory]
    [InlineData("users")]
    [InlineData("hash")]
    [InlineData("tenant")]
    [InlineData("branch")]
    [InlineData("roles")]
    [InlineData("duplicate")]
    [InlineData("expiry")]
    public async Task IncompleteConfiguration_CannotAuthenticate(string missing)
    {
        var options = ValidOptions();
        switch (missing)
        {
            case "users": options.Users.Clear(); break;
            case "hash": options.Users[0].PasswordHash = TestPassword; break;
            case "tenant": options.Users[0].TenantId = ""; break;
            case "branch": options.Users[0].BranchIds.Clear(); break;
            case "roles": options.Users[0].Roles = [AppRoles.PlatformAdmin]; break;
            case "duplicate": options.Users.Add(options.Users[0]); break;
            case "expiry": options.TokenExpirationMinutes = 480; break;
        }
        Assert.False(new HomologationAuthOptionsValidator(new TestEnvironment("Staging")).Validate(null, options).Succeeded);
        var service = new HomologationIdentityAuthenticationService(Options.Create(options), Options.Create(Jwt()), new TestEnvironment("Staging"));
        var result = await service.AuthenticateAsync("hml-test@example.invalid", TestPassword, default);
        Assert.Equal("identity.login.misconfigured", result.Error?.Code);
    }

    [Theory]
    [InlineData("Staging")]
    [InlineData("Production")]
    public async Task HostStartup_RejectsEnabledIncompleteConfiguration(string environment)
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { EnvironmentName = environment });
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Identity:HomologationAuth:Enabled"] = "true",
            ["Identity:Jwt:SigningKey"] = Jwt().SigningKey,
            ["Identity:Jwt:RequireHttpsMetadata"] = "true"
        });
        builder.Services.AddWAssisIdentity(builder.Configuration);
        using var host = builder.Build();
        await Assert.ThrowsAsync<OptionsValidationException>(() => host.StartAsync());
    }

    [Theory]
    [InlineData("Staging")]
    [InlineData("Production")]
    public void Registration_NeverFallsBackToDevelopment(string environment)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new TestEnvironment(environment));
        services.AddLogging();
        services.AddWAssisIdentity(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Identity:Jwt:SigningKey"] = Jwt().SigningKey,
            ["Identity:Jwt:RequireHttpsMetadata"] = "true",
            ["Identity:DevelopmentAuth:Enabled"] = "true"
        }).Build());
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.IsType<HomologationIdentityAuthenticationService>(scope.ServiceProvider.GetRequiredService<IIdentityAuthenticationService>());
    }

    private sealed class TestEnvironment(string environment) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environment;
        public string ApplicationName { get; set; } = "WAssis.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
