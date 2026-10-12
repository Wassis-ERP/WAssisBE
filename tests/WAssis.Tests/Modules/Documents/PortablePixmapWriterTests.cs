using System.Text;
using WAssis.Infra.Data.Integrations.Parsers;

namespace WAssis.Tests.Modules.Documents;

public sealed class PortablePixmapWriterTests
{
    [Fact]
    public async Task WritesP6Rgb_AndCompositesTransparentPixelsOnWhite()
    {
        var path = Path.Combine(Path.GetTempPath(), $"wassis-pixmap-{Guid.NewGuid():N}.ppm");
        try
        {
            await PortablePixmapWriter.WriteAsync(path, [0, 0, 255, 255, 0, 0, 0, 0], 2, 1, default);
            var bytes = await File.ReadAllBytesAsync(path);
            var header = Encoding.ASCII.GetBytes("P6\n2 1\n255\n");
            Assert.Equal(header, bytes[..header.Length]);
            Assert.Equal(new byte[] { 255, 0, 0, 255, 255, 255 }, bytes[header.Length..]);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task RejectsInconsistentRasterBeforeCreatingFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"wassis-pixmap-{Guid.NewGuid():N}.ppm");
        await Assert.ThrowsAsync<ArgumentException>(() => PortablePixmapWriter.WriteAsync(path, [0, 0, 0, 0], int.MaxValue, 2, default));
        Assert.False(File.Exists(path));
    }
}
