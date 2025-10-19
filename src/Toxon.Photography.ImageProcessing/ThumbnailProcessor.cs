using System.Text;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.S3;
using Amazon.S3.Model;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
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
    
    private async Task<(Stream Stream, IImageFormat Format, int Width, int Height)> ProcessImageAsync(Stream input, ThumbnailSettings settings)
    {
        var output = new MemoryStream();

        int width;
        int height;
        
        using (var image = await SixLabors.ImageSharp.Image.LoadAsync(input))
        {
            (width, height) = settings.CalculateDimensions(image.Width, image.Height);

            image.Mutate(x => x.Resize(width, height));
            
            await SixLabors.ImageSharp.ImageExtensions.SaveAsJpegAsync(image, output, new JpegEncoder { Quality = settings.Quality });
        }

        return (output, JpegFormat.Instance, width, height);
    }

    private async Task<string> UploadToS3Async(Stream thumbnail, IImageFormat format)
    {
        var thumbnailKey = "thumbnail/" + GenerateKey();
        await s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = BucketNames.Images,
            Key = thumbnailKey,
            InputStream = thumbnail,
            ContentType = format.DefaultMimeType,
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