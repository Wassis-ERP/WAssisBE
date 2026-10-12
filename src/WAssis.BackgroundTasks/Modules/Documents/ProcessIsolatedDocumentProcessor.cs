using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WAssis.Application.Modules.Documents.Interfaces;

namespace WAssis.BackgroundTasks.Modules.Documents;

public sealed class ProcessIsolatedDocumentProcessor(
    IPrivateDocumentStorage storage,
    IOptions<DocumentProcessingOptions> optionsAccessor) : IIsolatedDocumentProcessor
{
    private readonly DocumentProcessingOptions _options = optionsAccessor.Value;
    private readonly SemaphoreSlim _concurrency = new(optionsAccessor.Value.MaxConcurrency);

    public async Task<IsolatedDocumentProcessingResult> ProcessAsync(string storageKey, CancellationToken cancellationToken)
    {
        await _concurrency.WaitAsync(cancellationToken);
        var directory = Path.Combine(Path.GetTempPath(), "wassis-document-worker", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var input = Path.Combine(directory, "input.pdf");
        var output = Path.Combine(directory, "result.json");
        try
        {
            await using (var source = await storage.OpenReadAsync(storageKey, cancellationToken))
            await using (var target = new FileStream(input, FileMode.CreateNew, FileAccess.Write, FileShare.None, 64 * 1024,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
                await source.CopyToAsync(target, cancellationToken);

            using var process = Process.Start(BuildStartInfo(input, output))
                ?? throw new InvalidOperationException("Document parser process could not be started.");
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            using var cancellationRegistration = cancellationToken.Register(() => Kill(process));
            var started = Stopwatch.StartNew();
            while (!process.HasExited)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Kill(process);
                    cancellationToken.ThrowIfCancellationRequested();
                }
                await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                process.Refresh();
                if (process.HasExited) break;
                if (started.Elapsed > TimeSpan.FromSeconds(_options.TimeoutSeconds))
                {
                    Kill(process);
                    return new(null, null, "document_time_limit_exceeded");
                }
                if (process.WorkingSet64 > _options.MaxWorkingSetMb * 1024L * 1024L)
                {
                    Kill(process);
                    return new(null, null, "document_memory_limit_exceeded");
                }
                if (process.TotalProcessorTime > TimeSpan.FromSeconds(_options.MaxCpuSeconds))
                {
                    Kill(process);
                    return new(null, null, "document_cpu_limit_exceeded");
                }
            }
            if (process.ExitCode != 0 || !File.Exists(output))
                return new(null, null, "document_processor_failed");
            var info = new FileInfo(output);
            if (info.Length <= 0 || info.Length > _options.MaxOutputBytes)
                return new(null, null, "document_output_limit_exceeded");
            await using var resultStream = new FileStream(output, FileMode.Open, FileAccess.Read, FileShare.Read);
            return await JsonSerializer.DeserializeAsync<IsolatedDocumentProcessingResult>(resultStream, cancellationToken: cancellationToken)
                ?? new(null, null, "document_processor_failed");
        }
        finally
        {
            TryDeleteDirectory(directory);
            _concurrency.Release();
        }
    }

    private static ProcessStartInfo BuildStartInfo(string input, string output)
    {
        var info = new ProcessStartInfo
        {
            FileName = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet",
            WorkingDirectory = AppContext.BaseDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
        info.ArgumentList.Add(typeof(ProcessIsolatedDocumentProcessor).Assembly.Location);
        info.ArgumentList.Add("--document-parser");
        info.ArgumentList.Add(input);
        info.ArgumentList.Add(output);
        return info;
    }

    private static void Kill(Process process)
    {
        try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
        catch (InvalidOperationException) { }
        catch (System.ComponentModel.Win32Exception) { }
    }

    private static void TryDeleteDirectory(string directory)
    {
        try { if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
