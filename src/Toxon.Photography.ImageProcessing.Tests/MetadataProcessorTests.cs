namespace Toxon.Photography.ImageProcessing.Tests;

public class MetadataProcessorTests
{
    [Test]
    public async Task CanReadExifMetadata()
    {
        // the sample file is a preview image with all the exif metadata from a ARW file.
        // ```
        // exiftool -b -PreviewImage yourfile.ARW > preview.jpg
        // exiftool -TagsFromFile yourfile.ARW -all:all preview.jpg
        // ```
        
        await using var file = File.OpenRead("Fixtures/DSC02248.jpg");
        
        var metadata = await MetadataProcessor.ExtractMetadataAsync(file);
        
        Assert.That(metadata.Aperture, Is.EqualTo("f/4.0"));
    }
}