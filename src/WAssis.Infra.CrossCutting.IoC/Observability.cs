using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace WAssis.Infra.CrossCutting.IoC;

public static class Observability
{
    public static void AddWAssisObservability(this IServiceCollection services, IConfiguration configuration, string serviceName)
    {
        var export = !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName, serviceVersion: configuration["BUILD_SHA"] ?? "local"))
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation(options => options.RecordException = false)
                    .AddHttpClientInstrumentation(options => options.RecordException = false).AddSource("Npgsql", "WAssis.Workers")
                    .AddProcessor(new SafeTelemetryProcessor());
                if (export) tracing.AddOtlpExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddMeter("Npgsql", "WAssis.Workers")
                    .AddView("*", new MetricStreamConfiguration { TagKeys = ["http.request.method", "http.response.status_code", "http.route", "error.type", "worker", "provider", "outcome"] });
                if (export) metrics.AddOtlpExporter();
            });
    }
}

// Runs before exporters: exclude SQL, URLs, headers, exception messages, identities and payloads.
public sealed class SafeTelemetryProcessor : BaseProcessor<Activity>
{
    private static readonly HashSet<string> Allowed = ["http.request.method", "http.response.status_code", "http.route", "db.system", "db.system.name", "error.type", "wassis.correlation_id", "worker", "provider", "outcome"];
    public override void OnEnd(Activity activity)
    {
        foreach (var tag in activity.TagObjects.ToArray()) if (!Allowed.Contains(tag.Key)) activity.SetTag(tag.Key, null);
        activity.DisplayName = activity.Source.Name == "Npgsql" ? "PostgreSQL" : activity.GetTagItem("http.route") is string route ? $"HTTP {route}" : "Operation";
        if (activity.Status == ActivityStatusCode.Error) activity.SetStatus(ActivityStatusCode.Error);
    }
}
