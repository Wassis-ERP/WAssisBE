using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using WAssis.Application.Behaviors;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Application.Modules.Quotes.Services;

namespace WAssis.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly));
        services.AddValidatorsFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly, includeInternalTypes: true);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<IQuoteProcessingService, QuoteProcessingService>();

        return services;
    }
}
