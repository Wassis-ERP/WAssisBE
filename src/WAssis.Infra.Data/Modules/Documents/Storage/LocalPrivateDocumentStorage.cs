using Microsoft.Extensions.Options;
using WAssis.Application.Modules.Documents.Interfaces;
using WAssis.Infra.Data.Configuration;

namespace WAssis.Infra.Data.Modules.Documents.Storage;

public sealed class LocalPrivateDocumentStorage(IOptions<PrivateDocumentStorageOptions> optionsAccessor)
    : IPrivateDocumentStorage
{
    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();
    private readonly PrivateDocumentStorageOptions _options = optionsAccessor.Value;

    public async Task<string> StoreAsync(
        Guid documentId,
        string tenantId,
        string branchId,
        Stream content,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(tenantId, out var tenant) || !Guid.TryParse(branchId, out var branch))
            throw new UnauthorizedAccessException("Tenant e filial válidos são obrigatórios para armazenar documentos.");
        var key = $"{tenant:N}/{branch:N}/{documentId:N}.pdf";
        var path = ResolvePath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        try
        {
            await using var target = new FileStream(
                path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 64 * 1024,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            var buffer = new byte[64 * 1024];
            long total = 0;
            var signatureLength = 0;
            var signature = new byte[PdfSignature.Length];
            while (true)
            {
                var read = await content.ReadAsync(buffer, cancellationToken);
                if (read == 0) break;
                total += read;
                if (total > _options.MaxBytes) throw new InvalidDataException("document_size_limit_exceeded");
                if (signatureLength < signature.Length)
                {
                    var copy = Math.Min(read, signature.Length - signatureLength);
                    buffer.AsSpan(0, copy).CopyTo(signature.AsSpan(signatureLength));
                    signatureLength += copy;
                }
                await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            }
            if (total == 0 || !signature.AsSpan(0, signatureLength).StartsWith(PdfSignature))
                throw new InvalidDataException("document_signature_invalid");
            await target.FlushAsync(cancellationToken);
            return key;
        }
        catch
        {
            TryDelete(path);
            throw;
        }
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Stream stream = new FileStream(
            ResolvePath(storageKey), FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        TryDelete(ResolvePath(storageKey));
        return Task.CompletedTask;
    }

    private string ResolvePath(string storageKey)
    {
        var root = Path.GetFullPath(_options.RootPath);
        var path = Path.GetFullPath(Path.Combine(root, storageKey.Replace('/', Path.DirectorySeparatorChar)));
        var prefix = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("document_storage_key_invalid");
        return path;
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
