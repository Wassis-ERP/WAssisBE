namespace WAssis.Infra.Data.Configuration;

public sealed class PrivateDocumentStorageOptions
{
    public const string SectionName = "Documents:PrivateStorage";
    public string RootPath { get; set; } = string.Empty;
    public long MaxBytes { get; set; } = 8_000_000;
}
