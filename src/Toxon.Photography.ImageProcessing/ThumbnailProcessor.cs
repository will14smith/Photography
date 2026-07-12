using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using SkiaSharp;
using Toxon.Photography.Data;
using Toxon.Photography.Data.Config;

namespace Toxon.Photography.ImageProcessing;

public class ThumbnailProcessor(IAmazonS3 s3)
{
    public async Task<Image> Process(Image image, ThumbnailSettings settings)
    {
        var original = await GetImageStream(image);

        var (processed, format, width, height) = await ProcessImageAsync(original, settings);
        var key = await UploadToS3Async(processed, format);

        return new Image
        {
            Type = ImageType.Thumbnail,
            ObjectKey = key,
            Width = width,
            Height = height,
        };
    }

    private async Task<Stream> GetImageStream(Image image) => (await s3.GetObjectAsync(BucketNames.Images, image.ObjectKey)).ResponseStream;

    public static async Task<(Stream Stream, string MimeType, int Width, int Height)> ProcessImageAsync(Stream input, ThumbnailSettings settings) =>
        await Task.Run(() => ProcessImage(input, settings));

    public static (Stream Stream, string MimeType, int Width, int Height) ProcessImage(Stream input, ThumbnailSettings settings)
    {
        var output = new MemoryStream();
        
        using var original = SKBitmap.Decode(input);
        if (original == null)
        {
            throw new InvalidOperationException("Failed to decode image");
        }
        
        var (width, height) = settings.CalculateDimensions(original.Width, original.Height);

        using var resized = original.Resize(new SKImageInfo(width, height), SKSamplingOptions.Default);
        if (resized == null)
        {
            throw new InvalidOperationException("Failed to resize image");
        }

        using var data = resized.Encode(SKEncodedImageFormat.Jpeg, settings.Quality);
        data.SaveTo(output);
        output.Seek(0, SeekOrigin.Begin);
        
        return (output, "image/jpeg", width, height);
    }


    private async Task<string> UploadToS3Async(Stream thumbnail, string mimeType)
    {
        var thumbnailKey = "thumbnail/" + GenerateKey();
        await s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = BucketNames.Images,
            Key = thumbnailKey,
            InputStream = thumbnail,
            ContentType = mimeType,
        });
        return thumbnailKey;
    }

    private static string GenerateKey(int length = 40)
    {
        const string possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        var r = new Random();
        var sb = new StringBuilder(length);

        for (var i = 0; i < length; i++)
        {
            sb.Append(possible[r.Next(0, possible.Length)]);
        }

        return sb.ToString();
    }
}