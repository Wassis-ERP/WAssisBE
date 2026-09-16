using Serilog;
using WAssis.BackgroundTasks.Modules.Documents;
using WAssis.BackgroundTasks.Modules.Quotes;
using WAssis.BackgroundTasks;
using WAssis.Infra.CrossCutting.IoC;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Documents.Interfaces;

if (args.Length == 3 && string.Equals(args[0], "--document-parser", StringComparison.Ordinal))
{
    Environment.ExitCode = await DocumentParserProcess.RunAsync(args[1], args[2], CancellationToken.None);
    return;
}

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddWAssisDockerSecrets();
builder.Services.AddWAssisObservability(builder.Configuration, "WAssis.Worker");

builder.Services.AddSerilog((services, configuration) =>
    configuration.ReadFrom.Configuration(builder.Configuration));

NativeInjectorBootStrapper.RegisterServices(builder.Services, builder.Configuration, builder.Environment);
builder.Services.AddOptions<DocumentProcessingOptions>()
    .Bind(builder.Configuration.GetSection(DocumentProcessingOptions.SectionName))
    .Validate(options => options.TimeoutSeconds is >= 5 and <= 300
        && options.MaxCpuSeconds is >= 1 and <= 300
        && options.MaxWorkingSetMb is >= 128 and <= 2048
        && options.MaxOutputBytes is >= 100_000 and <= 4_000_000
        && options.MaxConcurrency is >= 1 and <= 8,
        "Documents:Processing limits are outside the supported safety bounds.")
    .ValidateOnStart();
builder.Services.AddSingleton<IIsolatedDocumentProcessor, ProcessIsolatedDocumentProcessor>();
builder.Services.AddScoped(provider => SystemDataScope.ForWorker("BackgroundTasks:durable-queues", purpose =>
    provider.GetRequiredService<ILogger<Worker>>().LogInformation("System data scope opened for {Purpose}", purpose)));
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<QuoteQuoteRequestDispatcherPlaceholder>();
builder.Services.AddHostedService<DocumentProcessingDispatcher>();

var host = builder.Build();
if (!builder.Environment.IsDevelopment())
{
    using var scope = host.Services.CreateScope();
    await WAssis.Infra.Data.Configuration.DatabaseMigrationGate.EnsureCurrentAsync(
        scope.ServiceProvider.GetRequiredService<WAssis.Infra.Data.Context.WAssisDbContext>());
}
host.Run();
