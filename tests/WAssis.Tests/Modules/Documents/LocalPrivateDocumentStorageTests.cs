using Microsoft.Extensions.Options;
using WAssis.Infra.Data.Configuration;
using WAssis.Infra.Data.Modules.Documents.Storage;

namespace WAssis.Tests.Modules.Documents;

public sealed class LocalPrivateDocumentStorageTests
{
    [Fact]
    public async Task StoreAsync_StreamsPdfToServerGeneratedPrivateKey()
    {
        var root = Path.Combine(Path.GetTempPath(), "wassis-storage-test", Guid.NewGuid().ToString("N"));
        try
        {
            var storage = Storage(root, 1024);
            var document = Guid.NewGuid();
            await using var content = new MemoryStream("%PDF-1.7 fixture"u8.ToArray());
            var key = await storage.StoreAsync(
                document,
                "22222222-2222-2222-2222-222222222222",
                "44444444-4444-4444-4444-444444444444",
                content,
                default);

            Assert.EndsWith($"{document:N}.pdf", key, StringComparison.Ordinal);
            Assert.DoesNotContain("fixture", key, StringComparison.OrdinalIgnoreCase);
            await using var reread = await storage.OpenReadAsync(key, default);
            Assert.Equal(content.Length, reread.Length);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("not-pdf")]
    [InlineData("")]
    public async Task StoreAsync_RejectsInvalidSignatureAndRemovesPartialFile(string value)
    {
        var root = Path.Combine(Path.GetTempPath(), "wassis-storage-test", Guid.NewGuid().ToString("N"));
        try
        {
            var storage = Storage(root, 1024);
            await using var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(value));
            await Assert.ThrowsAsync<InvalidDataException>(() => storage.StoreAsync(
                Guid.NewGuid(),
                "22222222-2222-2222-2222-222222222222",
                "44444444-4444-4444-4444-444444444444",
                content,
                default));
            Assert.False(Directory.Exists(root) && Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).Any());
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    private static LocalPrivateDocumentStorage Storage(string root, long maxBytes) => new(
        Options.Create(new PrivateDocumentStorageOptions { RootPath = root, MaxBytes = maxBytes }));
}
