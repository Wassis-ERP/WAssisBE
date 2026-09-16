using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using WAssis.Application.Modules.Documents.Interfaces;
using WAssis.Infra.Data.Configuration;
using WAssis.Infra.Data.Integrations.Parsers;
using UglyToad.PdfPig.Core;

namespace WAssis.BackgroundTasks.Modules.Documents;

internal static class DocumentParserProcess
{
    internal static async Task<int> RunAsync(string input, string output, CancellationToken cancellationToken)
    {
        IsolatedDocumentProcessingResult result;
        try
        {
            var info = new FileInfo(input);
            if (!info.Exists || info.Length is <= 0 or > 8_000_000)
                result = new(null, null, "document_size_limit_exceeded");
            else
            {
                var content = await File.ReadAllBytesAsync(input, cancellationToken);
                var text = await new PdfPigTextExtractor().ExtractTextAsync(content, cancellationToken);
                var origin = "pdf_text_layer";
                if (string.IsNullOrWhiteSpace(text))
                {
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: true)
                        .AddEnvironmentVariables()
                        .Build();
                    var ocrOptions = new TesseractOcrOptions();
                    configuration.GetSection(TesseractOcrOptions.SectionName).Bind(ocrOptions);
                    text = await new TesseractOcrTextExtractor(Options.Create(ocrOptions))
                        .ExtractTextAsync(content, cancellationToken);
                    origin = "ocr_fallback";
                }
                result = string.IsNullOrWhiteSpace(text)
                    ? new(null, null, "document_text_unavailable")
                    : new(new ProposalDocumentParser().Parse(text), origin, null);
            }
        }
        catch (InvalidDataException) { result = new(null, null, "document_invalid"); }
        catch (PdfDocumentFormatException) { result = new(null, null, "document_invalid"); }
        catch (IOException) { result = new(null, null, "document_read_error"); }
        catch (ArgumentException) { result = new(null, null, "document_invalid"); }
        catch (FormatException) { result = new(null, null, "document_format_unsupported"); }
        await File.WriteAllTextAsync(output, JsonSerializer.Serialize(result), cancellationToken);
        return 0;
    }
}
