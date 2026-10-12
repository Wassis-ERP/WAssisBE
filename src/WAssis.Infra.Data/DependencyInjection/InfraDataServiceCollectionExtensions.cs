using WAssis.Application.Modules.Calculations;
using WAssis.Infra.Data.Modules.Calculations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Npgsql;
using Microsoft.Extensions.Hosting;
using WAssis.Application.Modules.Billing.Interfaces;
using WAssis.Application.Modules.Customers.Interfaces;
using WAssis.Application.Modules.Core.Interfaces;
using WAssis.Application.Modules.Documents.Interfaces;
using WAssis.Application.Modules.Financial.Interfaces;
using WAssis.Application.Modules.Identity.Interfaces;
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
using WAssis.Infra.Data.Modules.Administration;
using WAssis.Application.Modules.Administration.Interfaces;
using WAssis.Infra.Data.Modules.Billing.Repositories;
using WAssis.Infra.Data.Modules.Customers.Repositories;
using WAssis.Infra.Data.Modules.Core.Queries;
using WAssis.Infra.Data.Modules.Documents.Repositories;
using WAssis.Infra.Data.Modules.Documents.Storage;
using WAssis.Infra.Data.Modules.Financial.Repositories;
using WAssis.Infra.Data.Modules.Identity;
using WAssis.Infra.Data.Modules.Notifications.Queries;
using WAssis.Infra.Data.Modules.Notifications.Repositories;
using WAssis.Infra.Data.Modules.Opportunities.Repositories;
using WAssis.Infra.Data.Modules.Policies.Repositories;
using WAssis.Infra.Data.Modules.Quotes.Repositories;
using WAssis.Infra.Data.Modules.WhatsAppSupport.Repositories;
using WAssis.Infra.Data.Transactions;
using WAssis.Infra.Data.Security;

namespace WAssis.Infra.Data.DependencyInjection;

public static class InfraDataServiceCollectionExtensions
{
    private static readonly TimeSpan ExternalAttemptTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan ExternalRequestTimeout = TimeSpan.FromSeconds(45);

    public static IServiceCollection AddInfraDataServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            if (!hostEnvironment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "ConnectionStrings:DefaultConnection must be configured outside Development.");
            }

            connectionString = "Host=localhost;Port=5432;Database=wassis;Username=postgres;Password=postgres";
        }

        if (!hostEnvironment.IsDevelopment()) DatabaseConnectionPolicy.ValidateSharedEnvironment(connectionString);

        // Authentication must not construct the request DbContext before HttpContext.User
        // has been assigned. Security queries lease separate, short-lived connections.
        services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));
        services.AddScoped<RecordPermissionScope>();
        services.AddDbContext<WAssisDbContext>((provider, options) =>
            options.UseNpgsql(provider.GetRequiredService<NpgsqlDataSource>(), npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "quotes")));

        services.Configure<TesseractOcrOptions>(options =>
            configuration.GetSection(TesseractOcrOptions.SectionName).Bind(options));
        services.AddOptions<AuthSessionOptions>()
            .Bind(configuration.GetSection(AuthSessionOptions.SectionName))
            .Validate(options => options.IdleTimeoutMinutes is >= 5 and <= 1440,
                "Identity:Session:IdleTimeoutMinutes must be between 5 and 1440 minutes.")
            .ValidateOnStart();
        services.AddOptions<PrivateDocumentStorageOptions>()
            .Bind(configuration.GetSection(PrivateDocumentStorageOptions.SectionName))
            .PostConfigure(options =>
            {
                if (hostEnvironment.IsDevelopment() && string.IsNullOrWhiteSpace(options.RootPath))
                    options.RootPath = Path.Combine(AppContext.BaseDirectory, "private-documents");
            })
            .Validate(options => Path.IsPathFullyQualified(options.RootPath),
                "Documents:PrivateStorage:RootPath must be an absolute private directory.")
            .Validate(options => options.MaxBytes is >= 1_000_000 and <= 20_000_000,
                "Documents:PrivateStorage:MaxBytes must be between 1 MB and 20 MB.")
            .ValidateOnStart();
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
        services.AddScoped<IAdministrationRepository, AdministrationRepository>();
        services.AddScoped<IEffectivePermissionService, EffectivePermissionService>();
        services.AddScoped<IExternalIdentityResolver, ExternalIdentityResolver>();
        services.AddScoped<IAuthSessionService, AuthSessionService>();
        services.AddScoped<ICoreBranchReadRepository, CoreBranchReadRepository>();
        services.AddScoped<IBusinessReferenceValidator, BusinessReferenceValidator>();
        services.AddScoped<ICoreCatalogReadRepository, CoreCatalogReadRepository>();
        services.AddScoped<IInsuredPersonRepository, InsuredPersonRepository>();
        services.AddScoped<IInsuredPersonReadRepository, InsuredPersonRepository>();
        services.AddScoped<IOpportunityRepository, OpportunityRepository>();
        services.AddScoped<ICanonicalCalculationStore, CanonicalCalculationStore>();
        services.AddScoped<ICanonicalExecutionStore, CanonicalExecutionStore>();
        services.AddScoped<ICanonicalPresentationStore, CanonicalPresentationStore>();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IOpportunityReadRepository, OpportunityRepository>();
        services.AddScoped<IOpportunityScope, OpportunityScope>();
        services.AddScoped<IDocumentSearchRepository, DocumentSearchRepository>();
        services.AddScoped<IImportedDocumentRepository, ImportedDocumentRepository>();
        services.AddSingleton<IPrivateDocumentStorage, LocalPrivateDocumentStorage>();
        services.AddScoped<ICommissionReceiptRepository, CommissionReceiptRepository>();
        services.AddSingleton<ICommissionStatementParser, CommissionStatementParser>();
        services.AddScoped<IOperationsDashboardReadRepository, OperationsDashboardReadRepository>();
        services.AddScoped<IPolicyDraftRepository, PolicyDraftRepository>();
        services.AddScoped<IQuoteProviderActivationRepository, QuoteProviderActivationRepository>();
        services.AddScoped<IQuoteRequestRepository, QuoteRequestRepository>();
        services.AddScoped<IDurableWorkQueue, WAssis.Infra.Data.Processing.DurableWorkQueue>();
        services.AddScoped<IWorkerDataScope, WorkerDataScope>();
        services.AddScoped<IWhatsAppConversationRepository, WhatsAppConversationRepository>();
        services.AddScoped<IAuditTrailWriter, AuditTrailWriter>();
        services.AddScoped<IApplicationTransaction, EfApplicationTransaction>();
        services.AddScoped<IDistributedOperationLimiter, PostgresOperationLimiter>();
        services.AddHttpClient<AggilizadorAutoQuoteClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AggilizadorAutoQuoteOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                client.BaseAddress = ExternalEndpointPolicy.RequireHttpsAllowlisted(options.BaseUrl, options.AllowedHosts);
            }
        })
        .AddStandardResilienceHandler(ConfigureExternalResilience);
        services.AddHttpClient<JustosBrokerAuthClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<JustosAutoQuoteOptions>>().Value;
            client.BaseAddress = ExternalEndpointPolicy.RequireHttpsAllowlisted(options.BaseUrl, options.AllowedHosts);
        })
        .AddStandardResilienceHandler(ConfigureExternalResilience);
        services.AddHttpClient<JustosQuoteClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<JustosAutoQuoteOptions>>().Value;
            client.BaseAddress = ExternalEndpointPolicy.RequireHttpsAllowlisted(options.BaseUrl, options.AllowedHosts);
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
        var defaultShouldHandle = options.Retry.ShouldHandle;
        options.Retry.ShouldHandle = arguments =>
        {
            var method = arguments.Context.GetRequestMessage()?.Method;
            return method == HttpMethod.Get || method == HttpMethod.Head || method == HttpMethod.Options
                ? defaultShouldHandle(arguments) : ValueTask.FromResult(false);
        };
        options.Retry.Delay = TimeSpan.FromSeconds(2);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.MinimumThroughput = 4;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(20);
    }
}
