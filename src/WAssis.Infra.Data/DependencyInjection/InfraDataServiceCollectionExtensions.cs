using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using WAssis.Application.Modules.Billing.Interfaces;
using WAssis.Application.Modules.Customers.Interfaces;
using WAssis.Application.Modules.Core.Interfaces;
using WAssis.Application.Modules.Documents.Interfaces;
using WAssis.Application.Modules.Financial.Interfaces;
using WAssis.Application.Modules.Notifications.Interfaces;
using WAssis.Application.Modules.Opportunities.Interfaces;
using WAssis.Application.Modules.Policies.Interfaces;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Application.Modules.WhatsAppSupport.Interfaces;
using WAssis.Application.Abstractions;
using WAssis.Infra.Data.Configuration;
using WAssis.Infra.Data.Context;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Aggilizador.Auto;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Aggilizador.Auto.Clients;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Bradesco.Auto;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Icatu;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Justos.Auto;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Justos.Auto.Clients;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Liberty.Auto;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Liberty.Business;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Liberty.Life;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Liberty.Residence;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Liberty.Travel;
using WAssis.Infra.Data.Integrations.Parsers;
using WAssis.Infra.Data.Modules.Billing.Repositories;
using WAssis.Infra.Data.Modules.Customers.Repositories;
using WAssis.Infra.Data.Modules.Core.Queries;
using WAssis.Infra.Data.Modules.Documents.Repositories;
using WAssis.Infra.Data.Modules.Financial.Repositories;
using WAssis.Infra.Data.Modules.Notifications.Queries;
using WAssis.Infra.Data.Modules.Notifications.Repositories;
using WAssis.Infra.Data.Modules.Opportunities.Repositories;
using WAssis.Infra.Data.Modules.Policies.Repositories;
using WAssis.Infra.Data.Modules.Quotes.Repositories;
using WAssis.Infra.Data.Modules.WhatsAppSupport.Repositories;

namespace WAssis.Infra.Data.DependencyInjection;

public static class InfraDataServiceCollectionExtensions
{
    private static readonly TimeSpan ExternalAttemptTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan ExternalRequestTimeout = TimeSpan.FromSeconds(45);

    public static IServiceCollection AddInfraDataServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=wassis;Username=postgres;Password=postgres";

        services.AddDbContext<WAssisDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "quotes")));

        services.Configure<TesseractOcrOptions>(options =>
            configuration.GetSection(TesseractOcrOptions.SectionName).Bind(options));
        services.Configure<AggilizadorAutoQuoteOptions>(options =>
            configuration.GetSection(AggilizadorAutoQuoteOptions.SectionName).Bind(options));
        services.Configure<BradescoAutoQuoteOptions>(options =>
            configuration.GetSection(BradescoAutoQuoteOptions.SectionName).Bind(options));
        services.Configure<IcatuQuoteOptions>(options =>
            configuration.GetSection(IcatuQuoteOptions.SectionName).Bind(options));
        services.Configure<LibertyAutoQuoteOptions>(options =>
            configuration.GetSection(LibertyAutoQuoteOptions.SectionName).Bind(options));
        services.Configure<LibertyBusinessQuoteOptions>(options =>
            configuration.GetSection(LibertyBusinessQuoteOptions.SectionName).Bind(options));
        services.Configure<LibertyLifeQuoteOptions>(options =>
            configuration.GetSection(LibertyLifeQuoteOptions.SectionName).Bind(options));
        services.Configure<LibertyResidenceQuoteOptions>(options =>
            configuration.GetSection(LibertyResidenceQuoteOptions.SectionName).Bind(options));
        services.Configure<LibertyTravelQuoteOptions>(options =>
            configuration.GetSection(LibertyTravelQuoteOptions.SectionName).Bind(options));
        services.Configure<JustosAutoQuoteOptions>(options =>
            configuration.GetSection(JustosAutoQuoteOptions.SectionName).Bind(options));
        services.AddScoped<IBillingRepository, BillingRepository>();
        services.AddScoped<ICoreBranchReadRepository, CoreBranchReadRepository>();
        services.AddScoped<IInsuredPersonRepository, InsuredPersonRepository>();
        services.AddScoped<IOpportunityRepository, OpportunityRepository>();
        services.AddScoped<IDocumentSearchRepository, DocumentSearchRepository>();
        services.AddScoped<IImportedDocumentRepository, ImportedDocumentRepository>();
        services.AddScoped<ICommissionReceiptRepository, CommissionReceiptRepository>();
        services.AddSingleton<ICommissionStatementParser, CommissionStatementParser>();
        services.AddScoped<IOperationsDashboardReadRepository, OperationsDashboardReadRepository>();
        services.AddScoped<IPolicyDraftRepository, PolicyDraftRepository>();
        services.AddScoped<IQuoteProviderActivationRepository, QuoteProviderActivationRepository>();
        services.AddScoped<IQuoteRequestRepository, QuoteRequestRepository>();
        services.AddScoped<IWhatsAppConversationRepository, WhatsAppConversationRepository>();
        services.AddScoped<IAuditTrailWriter, AuditTrailWriter>();
        services.AddHttpClient<AggilizadorAutoQuoteClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AggilizadorAutoQuoteOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                client.BaseAddress = new Uri(options.BaseUrl);
            }
        })
        .AddStandardResilienceHandler(ConfigureExternalResilience);
        services.AddHttpClient<JustosBrokerAuthClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<JustosAutoQuoteOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        })
        .AddStandardResilienceHandler(ConfigureExternalResilience);
        services.AddHttpClient<JustosQuoteClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<JustosAutoQuoteOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        })
        .AddStandardResilienceHandler(ConfigureExternalResilience);
        services.AddScoped<IQuoteProvider, AggilizadorAutoQuoteProvider>();
        services.AddScoped<IQuoteProvider, BradescoAutoQuoteProvider>();
        services.AddScoped<IQuoteProvider, IcatuQuoteProvider>();
        services.AddScoped<IQuoteProvider, LibertyAutoQuoteProvider>();
        services.AddScoped<IQuoteProvider, LibertyBusinessQuoteProvider>();
        services.AddScoped<IQuoteProvider, LibertyLifeQuoteProvider>();
        services.AddScoped<IQuoteProvider, LibertyResidenceQuoteProvider>();
        services.AddScoped<IQuoteProvider, LibertyTravelQuoteProvider>();
        services.AddScoped<IQuoteProvider, JustosAutoQuoteProvider>();
        services.AddSingleton<IPdfTextExtractor, PdfPigTextExtractor>();
        services.AddSingleton<IOcrTextExtractor, TesseractOcrTextExtractor>();
        services.AddSingleton<IProposalDocumentParser, ProposalDocumentParser>();

        return services;
    }

    private static void ConfigureExternalResilience(HttpStandardResilienceOptions options)
    {
        options.AttemptTimeout.Timeout = ExternalAttemptTimeout;
        options.TotalRequestTimeout.Timeout = ExternalRequestTimeout;
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.Delay = TimeSpan.FromSeconds(2);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.MinimumThroughput = 4;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(20);
    }
}
