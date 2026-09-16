extern alias background;

using Microsoft.Extensions.Options;
using WAssis.Infra.Data.Configuration;
using WAssis.Infra.Data.Modules.Documents.Storage;
using DocumentProcessingOptions = background::WAssis.BackgroundTasks.Modules.Documents.DocumentProcessingOptions;
using ProcessIsolatedDocumentProcessor = background::WAssis.BackgroundTasks.Modules.Documents.ProcessIsolatedDocumentProcessor;

namespace WAssis.Tests.Modules.Documents;

public sealed class ProcessIsolatedDocumentProcessorTests
{
    [Fact]
    public async Task ProcessAsync_ContainsCorruptPdfFailureInChildProcess()
    {
        var root = Path.Combine(Path.GetTempPath(), "wassis-isolated-parser-test", Guid.NewGuid().ToString("N"));
        try
        {
            var storage = new LocalPrivateDocumentStorage(Options.Create(new PrivateDocumentStorageOptions
            {
                RootPath = root,
                MaxBytes = 8_000_000,
            }));
            await using var content = new MemoryStream("%PDF-corrupt-fixture"u8.ToArray());
            var key = await storage.StoreAsync(
                Guid.NewGuid(),
                "22222222-2222-2222-2222-222222222222",
                "44444444-4444-4444-4444-444444444444",
                content,
                default);
            var processor = new ProcessIsolatedDocumentProcessor(storage, Options.Create(new DocumentProcessingOptions
            {
                TimeoutSeconds = 15,
                MaxCpuSeconds = 10,
                MaxWorkingSetMb = 512,
                MaxOutputBytes = 2_000_000,
                MaxConcurrency = 1,
            }));

            var result = await processor.ProcessAsync(key, default);

            Assert.False(result.Succeeded);
            Assert.Equal("document_invalid", result.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
