using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using WAssis.Application.Abstractions;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Infra.CrossCutting.Identity.Jwt;
using WAssis.Infra.CrossCutting.Identity.Models;
using WAssis.Infra.CrossCutting.Identity.Services;

namespace WAssis.Infra.CrossCutting.Identity.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWAssisIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtAccessOptions>(options =>
            configuration.GetSection(JwtAccessOptions.SectionName).Bind(options));

        var jwtOptions = new JwtAccessOptions();
        configuration.GetSection(JwtAccessOptions.SectionName).Bind(jwtOptions);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, HttpContextCurrentUserContext>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = jwtOptions.RequireHttpsMetadata;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = signingKey,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization(options =>
        {
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
