using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Infra.Data.Context;
using WAssis.Infra.Data.Modules.Quotes.Repositories;

namespace WAssis.Infra.Data.DependencyInjection;

public static class InfraDataServiceCollectionExtensions
{
    public static IServiceCollection AddInfraDataServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=wassis;Username=postgres;Password=postgres";

        services.AddDbContext<WAssisDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "quotes")));

        services.AddScoped<IQuoteRequestRepository, QuoteRequestRepository>();

        return services;
    }
}
