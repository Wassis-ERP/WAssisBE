using System.Globalization;
using System.Text;

namespace WAssis.Infra.Data.Integrations.Parsers;

// P6 PNM is natively supported by Tesseract/Leptonica. Input is PDFium's BGRA raster.
public static class PortablePixmapWriter
{
    public static async Task WriteAsync(string path, byte[] bgra, int width, int height, CancellationToken cancellationToken)
    {
        if (width <= 0 || height <= 0 || (long)width * height * 4 != bgra.LongLength)
            throw new ArgumentException("Invalid BGRA raster dimensions.");
        await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, true);
        var header = Encoding.ASCII.GetBytes(string.Create(CultureInfo.InvariantCulture, $"P6\n{width} {height}\n255\n"));
        await output.WriteAsync(header, cancellationToken);
        var row = new byte[checked(width * 3)];
        for (var y = 0; y < height; y++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var x = 0; x < width; x++)
            {
                var source = (y * width + x) * 4;
                var alpha = bgra[source + 3];
                // Composite transparency onto white paper, keeping RGB channel order.
                for (var channel = 0; channel < 3; channel++)
                    row[x * 3 + channel] = (byte)((bgra[source + 2 - channel] * alpha + 255 * (255 - alpha) + 127) / 255);
            }
            await output.WriteAsync(row, cancellationToken);
        }
    }
}
