using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.SNSEvents;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Toxon.Photography.Config;
using Toxon.Photography.Data;
using Toxon.Photography.Models;
using Image = SixLabors.ImageSharp.Image;

namespace Toxon.Photography
{
    public class ThumbnailProcessorFunction
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly IAmazonS3 _s3;

        private readonly ThumbnailSettings _thumbnailSettings = new ThumbnailSettings(width: null, height: 250, quality: 90);

        public ThumbnailProcessorFunction()
            : this(new AmazonDynamoDBClient(), new AmazonS3Client())
        {
        }

        public ThumbnailProcessorFunction(IAmazonDynamoDB dynamoDb, IAmazonS3 s3)
        {
            _dynamoDb = dynamoDb;
            _s3 = s3;
        }

        public async Task Handle(SNSEvent snsEvent)
        {
            foreach (var record in snsEvent.Records)
            {
                var rawMessage = record.Sns.Message;
                var message = JsonConvert.DeserializeObject<ImageProcessorMessage>(rawMessage);

                await HandleMessageAsync(message);
            }
        }

        private async Task HandleMessageAsync(ImageProcessorMessage message)
        {
            var stream = await GetImageFromS3(message.Image.ObjectKey);
            var (thumbnail, format) = ProcessImage(stream);
            var thumbnailKey = await UploadImageToS3(thumbnail, format);
            thumbnail.Dispose();
            
            await UpdatePhotographInDatabase(message.PhotographId, thumbnailKey);
        }

        private async Task<Stream> GetImageFromS3(string objectKey)
        {
            var obj = await _s3.GetObjectAsync(BucketNames.Images, objectKey);
            return obj.ResponseStream;
        }

        private (Stream, IImageFormat) ProcessImage(Stream input)
        {
            var output = new MemoryStream();

            using (var image = Image.Load(input))
            {
                var (width, height) = _thumbnailSettings.CalculateDimensions(image.Width, image.Height);

                image.Mutate(x =>
                    x.Resize(width, height));

                image.SaveAsJpeg(output, new JpegEncoder { Quality = _thumbnailSettings.Quality });
            }

            return (output, JpegFormat.Instance);
        }
        
        private async Task<string> UploadImageToS3(Stream thumbnail, IImageFormat format)
        {
            var thumbnailKey = "thumbnail/" + GenerateKey();
            await _s3.PutObjectAsync(new PutObjectRequest
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
            var sb = new StringBuilder();

            for (var i = 0; i < length; i++)
            {
                sb.Append(possible[r.Next(0, possible.Length)]);
            }

            return sb.ToString();
        }

        private async Task UpdatePhotographInDatabase(Guid photographId, string thumbnailKey)
        {
            var image = new Data.Image { Type = ImageType.Thumbnail, ObjectKey = thumbnailKey };
            var imageDocument = ImageSerialization.ToDocument(image);

            await _dynamoDb.UpdateItemAsync(new UpdateItemRequest
            {
                TableName = TableNames.Photograph,

                Key = new Dictionary<string, AttributeValue> { { "id", new AttributeValue { S = photographId.ToString() } } },
                UpdateExpression = "SET #images = list_append(#images, :thumbnails)",
                ExpressionAttributeNames = new Dictionary<string, string> { { "#images", "images" } },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":thumbnails", new AttributeValue { L = new List<AttributeValue> { new AttributeValue { M = imageDocument.ToAttributeMap() } } } } }
            });
        }
    }
}
