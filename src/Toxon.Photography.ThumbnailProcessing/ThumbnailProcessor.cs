using System.Text;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Amazon.S3;
using Amazon.S3.Model;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Toxon.Photography.Data;
using Toxon.Photography.Data.Config;

namespace Toxon.Photography.ThumbnailProcessing;

public class ThumbnailProcessor(IAmazonDynamoDB dynamoDb, IAmazonEventBridge eventBridge, IAmazonS3 s3)
{
    private readonly ITable _photographTable = PhotographTable.Create(dynamoDb);
    private readonly ThumbnailSettings _thumbnailSettings = new(width: 850, height: null, quality: 90);

    public async Task Process(Guid id)
    {
        var photograph = await GetPhotographFromDatabase(id);
        if (photograph.Images.Any(x => x.Type == ImageType.Thumbnail))
        {
            return;
        }

        var image = photograph.Images.Single(x => x.Type == ImageType.Full);
        var imageStream = await GetImageStream(image);
        
        var (thumbnailStream, format) = ProcessImage(imageStream);
        var thumbnailKey = await UploadThumbnailToS3(thumbnailStream, format);
        var thumbnail = new Image { Type = ImageType.Thumbnail, ObjectKey = thumbnailKey };

        await AddThumbnailToPhotographInDatabase(photograph, thumbnail);

        photograph.Images = photograph.Images.Append(thumbnail).ToList();
        await SendEvent(photograph);
    }
    
    private async Task<Photograph> GetPhotographFromDatabase(Guid id) => PhotographSerialization.FromDocument(await _photographTable.GetItemAsync(id));

    private async Task<Stream> GetImageStream(Image image) => (await s3.GetObjectAsync(BucketNames.Images, image.ObjectKey)).ResponseStream;
    
    private (Stream, IImageFormat) ProcessImage(Stream input)
    {
        var output = new MemoryStream();

        using (var image = SixLabors.ImageSharp.Image.Load(input))
        {
            var (width, height) = _thumbnailSettings.CalculateDimensions(image.Width, image.Height);

            image.Mutate(x => x.Resize(width, height));
            
            SixLabors.ImageSharp.ImageExtensions.SaveAsJpeg(image, output, new JpegEncoder { Quality = _thumbnailSettings.Quality });
        }

        return (output, JpegFormat.Instance);
    }

    private async Task<string> UploadThumbnailToS3(Stream thumbnail, IImageFormat format)
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
    
    private async Task AddThumbnailToPhotographInDatabase(Photograph photograph, Image thumbnail)
    {
        var thumbnailDocument = ImageSerialization.ToDocument(thumbnail);

        await dynamoDb.UpdateItemAsync(new UpdateItemRequest
        {
            TableName = TableNames.Photograph,

            Key = new Dictionary<string, AttributeValue> { { "id", new AttributeValue { S = photograph.Id.ToString() } } },
            UpdateExpression = "SET #images = list_append(#images, :thumbnails)",
            ExpressionAttributeNames = new Dictionary<string, string> { { "#images", "images" } },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":thumbnails", new AttributeValue { L = [new AttributeValue { M = thumbnailDocument.ToAttributeMap() }] } } }
        });
    }
    
    public async Task SendEvent(Photograph photograph)
    {
        var eventDetail = new PhotographEvent { Photograph = photograph };
        
        await eventBridge.PutEventsAsync(new PutEventsRequest
        {
            Entries = [
                new PutEventsRequestEntry
                {
                    Detail = JsonSerializer.Serialize(eventDetail),
                    DetailType = $"photograph.update",
                    Source = "photography",
                }
            ]
        });
    }
}