using System.Diagnostics.Metrics;

namespace WAssis.Application.Abstractions;

public static class WorkerMetrics
{
    public static readonly System.Diagnostics.ActivitySource Traces = new("WAssis.Workers");
    private static readonly Meter Meter = new("WAssis.Workers");
    public static readonly Counter<long> Processed = Meter.CreateCounter<long>("wassis.worker.processed");
    public static readonly Counter<long> ProviderFailures = Meter.CreateCounter<long>("wassis.quote.provider_failures");
    public static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("wassis.worker.duration", "s");
    private static long _pending;
    private static double _age;
    static WorkerMetrics()
    {
        Meter.CreateObservableGauge("wassis.quote.pending", () => Interlocked.Read(ref _pending));
        Meter.CreateObservableGauge("wassis.quote.oldest_pending_age", () => Volatile.Read(ref _age), "s");
    }
    public static void RecordQueue(long count, DateTime? oldest)
    {
        Interlocked.Exchange(ref _pending, count);
        Volatile.Write(ref _age, oldest.HasValue ? Math.Max(0, (DateTime.UtcNow - oldest.Value).TotalSeconds) : 0);
    }
}
