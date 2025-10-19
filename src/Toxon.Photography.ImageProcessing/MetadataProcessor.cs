using System.Globalization;
using Amazon.S3;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using Toxon.Photography.Data.Config;

namespace Toxon.Photography.ImageProcessing;

public class MetadataProcessor(IAmazonS3 s3)
{
    public async Task<Metadata> ProcessAsync(string imageKey)
    {
        var image = await GetImageStreamAsync(imageKey);

        return await ExtractMetadataAsync(image);
    }

    private async Task<Metadata> ExtractMetadataAsync(Stream input)
    {
        using var image = await SixLabors.ImageSharp.Image.LoadAsync(input);
        
        var metadata = new Metadata
        {
            Width = image.Width,
            Height = image.Height
        };
        
        if (image.Metadata.ExifProfile != null)
        {
            PopulateFromExif(metadata, image.Metadata.ExifProfile);
        }

        return metadata;
    }

    private static void PopulateFromExif(Metadata metadata, ExifProfile exif)
    {
        if (exif.TryGetValue(ExifTag.DateTimeOriginal, out var dateTimeOriginal) && dateTimeOriginal.Value is not null)
        {
            if (DateTime.TryParseExact(dateTimeOriginal.Value, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDateTimeOriginal))
            {
                metadata.CaptureTime = parsedDateTimeOriginal;
            }
        }
            
        if (exif.TryGetValue(ExifTag.ExposureTime, out var exposureTime))
        {
            var value = exposureTime.Value.Numerator / (decimal)exposureTime.Value.Denominator;
            
            metadata.Exposure = $"{(value < 0 ? exposureTime.Value.ToString() : value.ToString("0.0"))} sec";
        }
            
        if (exif.TryGetValue(ExifTag.FNumber, out var fNumber))
        {
            var value = fNumber.Value.Numerator / (decimal)fNumber.Value.Denominator;
            metadata.Aperture = $"f/{value:0.0}";
        }
            
        if (exif.TryGetValue(ExifTag.FocalLength, out var focalLength))
        {
            var value = focalLength.Value.Numerator / (decimal)focalLength.Value.Denominator;
            metadata.FocalLength = $"{value:0}mm";
        }
            
        if (exif.TryGetValue(ExifTag.ISOSpeed, out var isoSpeed))
        {
            metadata.ISO = $"ISO {isoSpeed.Value}";
        }
            
        if ((exif.TryGetValue(ExifTag.LensMake, out var lensMake) && lensMake.Value is not null) |
            (exif.TryGetValue(ExifTag.LensModel, out var lensModel) && lensModel.Value is not null))
        {
            var lensMakeValue = lensMake?.Value ?? string.Empty;
            var lensModelValue = lensModel?.Value ?? string.Empty;
            metadata.Lens = $"{lensMakeValue} {lensModelValue}".Trim();
        }

        if ((exif.TryGetValue(ExifTag.Make, out var cameraMake) && cameraMake.Value is not null) |
            (exif.TryGetValue(ExifTag.Model, out var cameraModel) && cameraModel.Value is not null))
        {
            var cameraMakeValue = cameraMake?.Value ?? string.Empty;
            var cameraModelValue = cameraModel?.Value ?? string.Empty;
            metadata.Camera = $"{cameraMakeValue} {cameraModelValue}".Trim();
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