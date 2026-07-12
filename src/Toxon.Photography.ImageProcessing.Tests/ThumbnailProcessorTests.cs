using SkiaSharp;

namespace Toxon.Photography.ImageProcessing.Tests;

public class ThumbnailProcessorTests
{
    [Test]
    public async Task CanResizeImage()
    {
        await using var file = File.OpenRead("Fixtures/DSC02248.jpg");

        var (stream, _, _, _) = await ThumbnailProcessor.ProcessImageAsync(file, new ThumbnailSettings(100, null, 100));

        var image = SKBitmap.Decode(stream);

        Assert.Multiple(() =>
        {
            Assert.That(image.Width, Is.EqualTo(100));
            Assert.That(image.Height, Is.EqualTo(67));
        });
    }
}