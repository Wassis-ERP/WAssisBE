namespace WAssis.BackgroundTasks.Modules.Documents;

public sealed class DocumentProcessingOptions
{
    public const string SectionName = "Documents:Processing";
    public int TimeoutSeconds { get; set; } = 45;
    public int MaxCpuSeconds { get; set; } = 30;
    public int MaxWorkingSetMb { get; set; } = 512;
    public int MaxOutputBytes { get; set; } = 2_000_000;
    public int MaxConcurrency { get; set; } = 2;
}
