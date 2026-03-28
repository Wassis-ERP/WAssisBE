using System.Diagnostics;
using Docnet.Core;
using Docnet.Core.Models;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WAssis.Application.Modules.Documents.Interfaces;
using WAssis.Infra.Data.Configuration;

namespace WAssis.Infra.Data.Integrations.Parsers;

public sealed class TesseractOcrTextExtractor(IOptions<TesseractOcrOptions> optionsAccessor) : IOcrTextExtractor
{
    private readonly TesseractOcrOptions _options = optionsAccessor.Value;

    public async Task<string?> ExtractTextAsync(byte[] content, CancellationToken cancellationToken)
    {
        var executablePath = ResolveExecutablePath();
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return null;
        }

        var workingDirectory = Path.Combine(Path.GetTempPath(), "wassis-ocr", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workingDirectory);

        try
        {
            var pageTexts = new List<string>();

            using var documentReader = DocLib.Instance.GetDocReader(
                content,
                new PageDimensions(_options.RenderWidth, _options.RenderHeight));

            var pageCount = Math.Min(documentReader.GetPageCount(), Math.Max(_options.MaxPages, 1));
            for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var pageReader = documentReader.GetPageReader(pageIndex);
                var rawBytes = pageReader.GetImage();
                var width = pageReader.GetPageWidth();
                var height = pageReader.GetPageHeight();

                if (rawBytes.Length == 0 || width == 0 || height == 0)
                {
                    continue;
                }

                var imagePath = Path.Combine(workingDirectory, $"page-{pageIndex + 1:D2}.png");
                var outputBasePath = Path.Combine(workingDirectory, $"page-{pageIndex + 1:D2}");

                using (var image = Image.LoadPixelData<Bgra32>(rawBytes, width, height))
                {
                    await image.SaveAsPngAsync(imagePath, cancellationToken);
                }

                var extractedText = await RunTesseractAsync(executablePath, imagePath, outputBasePath, cancellationToken);
                if (!string.IsNullOrWhiteSpace(extractedText))
                {
                    pageTexts.Add(extractedText.Trim());
                }
            }

            return pageTexts.Count == 0 ? null : string.Join(Environment.NewLine + Environment.NewLine, pageTexts);
        }
        finally
        {
            TryDeleteDirectory(workingDirectory);
        }
    }

    private async Task<string?> RunTesseractAsync(
        string executablePath,
        string imagePath,
        string outputBasePath,
        CancellationToken cancellationToken)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        processStartInfo.ArgumentList.Add(imagePath);
        processStartInfo.ArgumentList.Add(outputBasePath);
        processStartInfo.ArgumentList.Add("-l");
        processStartInfo.ArgumentList.Add(_options.Language);
        processStartInfo.ArgumentList.Add("--psm");
        processStartInfo.ArgumentList.Add(_options.PageSegmentationMode);
        processStartInfo.ArgumentList.Add("quiet");

        using var process = new Process { StartInfo = processStartInfo };
        process.Start();

        var timeout = TimeSpan.FromSeconds(Math.Max(_options.TimeoutSeconds, 5));
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);

        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            TryKill(process);
            return null;
        }

        var outputPath = $"{outputBasePath}.txt";
        if (!File.Exists(outputPath))
        {
            return null;
        }

        return await File.ReadAllTextAsync(outputPath, cancellationToken);
    }

    private string? ResolveExecutablePath()
    {
        if (!string.IsNullOrWhiteSpace(_options.ExecutablePath) && File.Exists(_options.ExecutablePath))
        {
            return _options.ExecutablePath;
        }

        return FindOnPath("tesseract.exe") ?? FindOnPath("tesseract");
    }

    private static string? FindOnPath(string fileName)
    {
        var environmentPath = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(environmentPath))
        {
            return null;
        }

        foreach (var segment in environmentPath.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(segment.Trim(), fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // Intentionally swallow cleanup failures in OCR helper.
        }
    }

    private static void TryDeleteDirectory(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
        catch
        {
            // Intentionally swallow temp cleanup failures.
        }
    }
}
