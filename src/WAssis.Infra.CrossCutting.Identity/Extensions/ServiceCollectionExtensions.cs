using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Identity.Interfaces;
using WAssis.Infra.CrossCutting.Identity.Authentication;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Infra.CrossCutting.Identity.Jwt;
using WAssis.Infra.CrossCutting.Identity.Models;
using WAssis.Infra.CrossCutting.Identity.Services;

namespace WAssis.Infra.CrossCutting.Identity.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWAssisIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<JwtAccessOptions>, JwtAccessOptionsValidator>();
        services
            .AddOptions<JwtAccessOptions>()
            .Bind(configuration.GetSection(JwtAccessOptions.SectionName))
            .ValidateOnStart();
        services.Configure<JwtAccessOptions>(options =>
            configuration.GetSection(JwtAccessOptions.SectionName).Bind(options));
        services.Configure<DevelopmentAuthOptions>(options =>
            configuration.GetSection(DevelopmentAuthOptions.SectionName).Bind(options));
        services.AddSingleton<IValidateOptions<HomologationAuthOptions>, HomologationAuthOptionsValidator>();
        services.AddOptions<HomologationAuthOptions>()
            .Bind(configuration.GetSection(HomologationAuthOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<Auth0Options>, Auth0OptionsValidator>();
        services.AddOptions<Auth0Options>()
            .Bind(configuration.GetSection(Auth0Options.SectionName))
            .ValidateOnStart();

        var jwtOptions = new JwtAccessOptions();
        configuration.GetSection(JwtAccessOptions.SectionName).Bind(jwtOptions);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));
        var validationKeys = jwtOptions.PreviousSigningKeys
            .Prepend(jwtOptions.SigningKey)
            .Select(static key => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)))
            .Cast<SecurityKey>()
            .ToArray();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, HttpContextCurrentUserContext>();
        services.AddScoped<DevelopmentIdentityAuthenticationService>();
        services.AddScoped<HomologationIdentityAuthenticationService>();
        services.AddScoped<ProductionPasswordAuthenticationService>();
        services.AddScoped<IInternalAccessTokenIssuer, InternalAccessTokenIssuer>();
        services.AddScoped<IIdentityAuthenticationService>(provider =>
        {
            var environment = provider.GetRequiredService<IHostEnvironment>();
            if (environment.IsDevelopment()) return provider.GetRequiredService<DevelopmentIdentityAuthenticationService>();
            if (environment.IsStaging()) return provider.GetRequiredService<HomologationIdentityAuthenticationService>();
            return provider.GetRequiredService<ProductionPasswordAuthenticationService>();
        });

        var authentication = services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = jwtOptions.RequireHttpsMetadata;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKeys = validationKeys,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (string.IsNullOrWhiteSpace(context.Token)
                            && context.Request.Cookies.TryGetValue("wassis_access", out var cookieToken))
                            context.Token = cookieToken;
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        var principal = context.Principal;
                        var jti = principal?.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti);
                        var userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal?.FindFirstValue("sub");
                        var tenantId = principal?.FindFirstValue(ClaimConstants.TenantId);
                        var sessions = context.HttpContext.RequestServices.GetRequiredService<IAuthSessionService>();
                        if (string.IsNullOrWhiteSpace(jti) || string.IsNullOrWhiteSpace(userId)
                            || string.IsNullOrWhiteSpace(tenantId))
                        {
                            context.Fail("Sessão revogada, expirada ou sem vínculo ativo.");
                            return;
                        }
                        var branches = await sessions.GetAuthorizedBranchesAsync(jti, userId, tenantId, context.HttpContext.RequestAborted);
                        if (branches.Count == 0 || principal?.Identity is not ClaimsIdentity identity)
                        {
                            context.Fail("Sessão revogada, expirada ou sem vínculo ativo.");
                            return;
                        }
                        // The signed token identifies the session. Branch bindings are live
                        // database permissions, including grants/revocations since login.
                        var previousBranch = identity.FindFirst(ClaimConstants.BranchId)?.Value;
                        foreach (var claim in identity.Claims.Where(c => c.Type is ClaimConstants.BranchId or ClaimConstants.BranchIds).ToArray())
                            identity.RemoveClaim(claim);
                        foreach (var branch in branches) identity.AddClaim(new Claim(ClaimConstants.BranchIds, branch));
                        identity.AddClaim(new Claim(ClaimConstants.BranchId,
                            previousBranch is not null && branches.Contains(previousBranch) ? previousBranch : branches[0]));
                    },
                };
            })
            .AddCookie(ExternalAuthenticationSchemes.TemporaryCookie, options =>
            {
                options.Cookie.Name = "wassis_external";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.SlidingExpiration = false;
            });

        var auth0Options = new Auth0Options();
        configuration.GetSection(Auth0Options.SectionName).Bind(auth0Options);
        if (auth0Options.Enabled)
        {
            authentication.AddOpenIdConnect(ExternalAuthenticationSchemes.Auth0, options =>
            {
                options.Authority = auth0Options.Authority.TrimEnd('/');
                options.ClientId = auth0Options.ClientId;
                options.ClientSecret = auth0Options.ClientSecret;
                options.ResponseType = "code";
                options.UsePkce = true;
                options.RequireHttpsMetadata = true;
                options.MapInboundClaims = false;
                options.SaveTokens = false;
                options.SignInScheme = ExternalAuthenticationSchemes.TemporaryCookie;
                options.CallbackPath = "/signin-auth0";
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.ClaimActions.Remove("amr");
                options.TokenValidationParameters.NameClaimType = "name";
                options.TokenValidationParameters.RoleClaimType = "roles";
            });
        }

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy(AccessPolicies.AuthenticatedUser, policy =>
                policy.RequireAuthenticatedUser());

            options.AddPolicy(AccessPolicies.PlatformAdmin, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireRole(AppRoles.PlatformAdmin));

            options.AddPolicy(AccessPolicies.BrokerageStaff, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(ClaimConstants.UserType, UserTypes.BrokerageStaff));

            options.AddPolicy(AccessPolicies.BrokerageAdmin, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(ClaimConstants.UserType, UserTypes.BrokerageStaff)
                    .RequireRole(AppRoles.BrokerageOwner, AppRoles.BrokerageAdmin, AppRoles.BrokerageManager));

            options.AddPolicy(AccessPolicies.DigitalCustomer, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(ClaimConstants.UserType, UserTypes.Customer)
                    .RequireRole(AppRoles.DigitalCustomer, AppRoles.EndCustomer));
        });

        return services;
    }
}
