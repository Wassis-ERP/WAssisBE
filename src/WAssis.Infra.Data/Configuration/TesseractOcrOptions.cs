namespace WAssis.Infra.Data.Configuration;

public sealed class TesseractOcrOptions
{
    public const string SectionName = "Ocr";

    public string? ExecutablePath { get; set; }
    public string Language { get; set; } = "por+eng";
    public int MaxPages { get; set; } = 3;
    public int RenderWidth { get; set; } = 1600;
    public int RenderHeight { get; set; } = 2200;
    public int TimeoutSeconds { get; set; } = 30;
    public string PageSegmentationMode { get; set; } = "6";
}
