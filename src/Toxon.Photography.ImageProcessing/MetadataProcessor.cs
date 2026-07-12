using System.Diagnostics;
using System.Globalization;
using Amazon.S3;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using SkiaSharp;
using Toxon.Photography.Data.Config;

namespace Toxon.Photography.ImageProcessing;

public class MetadataProcessor(IAmazonS3 s3)
{
    public async Task<Metadata> ProcessAsync(string imageKey)
    {
        var image = await GetImageStreamAsync(imageKey);

        return await ExtractMetadataAsync(image);
    }

    public async static Task<Metadata> ExtractMetadataAsync(Stream input)
    {
        using var memoryStream = new MemoryStream();
        await input.CopyToAsync(memoryStream);
        
        memoryStream.Seek(0, SeekOrigin.Begin);
        var directories = ImageMetadataReader.ReadMetadata(memoryStream);
        
        memoryStream.Seek(0, SeekOrigin.Begin);
        using var image = SKBitmap.Decode(memoryStream);
        
        var metadata = new Metadata
        {
            Width = image.Width,
            Height = image.Height
        };
        
        foreach (var directory in directories)
        {
            switch (directory)
            {
                case ExifSubIfdDirectory exif:
                    PopulateFromExif(metadata, exif);
                    break;
                
                default:
                    Debug.WriteLine("Skipping unknown directory: {0}", directory);
                    break;
            }
        }
        
        return metadata;
    }

    private static void PopulateFromExif(Metadata metadata, ExifSubIfdDirectory exif)
    {
        if (exif.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dateTimeOriginal))
        {
            metadata.CaptureTime = dateTimeOriginal;
        }
            
        if (exif.TryGetRational(ExifDirectoryBase.TagExposureTime, out var exposureTime))
        {
            var value = exposureTime.Numerator / (decimal)exposureTime.Denominator;
            
            metadata.Exposure = $"{(value < 0 ? exposureTime.ToString(CultureInfo.InvariantCulture) : value.ToString("0.0"))} sec";
        }
            
        if (exif.TryGetRational(ExifDirectoryBase.TagFNumber, out var fNumber))
        {
            var value = fNumber.Numerator / (decimal)fNumber.Denominator;
            metadata.Aperture = $"f/{value:0.0}";
        }
            
        if (exif.TryGetRational(ExifDirectoryBase.TagFocalLength, out var focalLength))
        {
            var value = focalLength.Numerator / (decimal)focalLength.Denominator;
            metadata.FocalLength = $"{value:0}mm";
        }
            
        if (exif.TryGetInt64(ExifDirectoryBase.TagIsoSpeed, out var isoSpeed))
        {
            metadata.ISO = $"ISO {isoSpeed}";
        }
            
        var lensMake = exif.GetString(ExifDirectoryBase.TagLensMake);
        var lensModel = exif.GetString(ExifDirectoryBase.TagLensModel);
        
        if (lensMake  is not null ||  lensModel is not null)
        {
            metadata.Lens = $"{lensMake ?? string.Empty} {lensModel ?? string.Empty}".Trim();
        }

        var cameraMake = exif.GetString(ExifDirectoryBase.TagMake);
        var cameraModel = exif.GetString(ExifDirectoryBase.TagModel);
        
        if (cameraMake is not null || cameraModel is not null)
        {
            metadata.Camera = $"{cameraMake ?? string.Empty} {cameraModel ?? string.Empty}".Trim();
        }
    }

    private async Task<Stream> GetImageStreamAsync(string imageKey) => (await s3.GetObjectAsync(BucketNames.Images, imageKey)).ResponseStream;
}

public class Metadata
{
    public required int Width { get; init; }
    public required int Height { get; init; }
    
    public DateTime? CaptureTime { get; set; }
    public string? Camera { get; set; }
    public string? Lens { get; set; }
    public string? Exposure { get; set; }
    public string? Aperture { get; set; }
    public string? FocalLength { get; set; }
    public string? ISO { get; set; }

    public IReadOnlyDictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        
        if (!string.IsNullOrEmpty(Camera)) { dict["Camera"] = Camera; }
        if (!string.IsNullOrEmpty(Lens)) { dict["Lens"] = Lens; }
        if (!string.IsNullOrEmpty(Exposure)) { dict["Exposure"] = Exposure; }
        if (!string.IsNullOrEmpty(Aperture)) { dict["Aperture"] = Aperture; }
        if (!string.IsNullOrEmpty(FocalLength)) { dict["FocalLength"] = FocalLength; }
        if (!string.IsNullOrEmpty(ISO)) { dict["ISO"] = ISO; }

        return dict;
    }
}