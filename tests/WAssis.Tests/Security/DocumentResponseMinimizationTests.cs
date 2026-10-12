using WAssis.Services.Api.Modules.Documents.ViewModels;

namespace WAssis.Tests.Security;

public sealed class DocumentResponseMinimizationTests
{
    [Fact]
    public void ImportedDocumentResponse_DoesNotExposePrivateStorageKey()
    {
        Assert.Null(typeof(ImportedDocumentViewModel).GetProperty("StoragePath"));
    }
}
