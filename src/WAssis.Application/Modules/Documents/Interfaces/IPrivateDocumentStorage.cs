namespace WAssis.Application.Modules.Documents.Interfaces;

public interface IPrivateDocumentStorage
{
    Task<string> StoreAsync(
        Guid documentId,
        string tenantId,
        string branchId,
        Stream content,
        CancellationToken cancellationToken);
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken);
}
