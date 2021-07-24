using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Toxon.Photography.Config;
using Toxon.Photography.Data;
using Toxon.Photography.Generation;
using Toxon.Photography.Generation.Models;
using File = Toxon.Photography.Generation.Models.File;
using Filter = Amazon.S3.Model.Filter;

namespace Toxon.Photography
{
    public class SiteGeneratorFunction
    {
        private static readonly TimeSpan ExpirationPeriod = TimeSpan.FromDays(2);

        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly IAmazonS3 _s3Site;
        private readonly IAmazonS3 _s3Images;

        public SiteGeneratorFunction()
            : this(new AmazonDynamoDBClient(), new AmazonS3Client(), NewImagesClient().Result)
        {
        }

        private static async Task<IAmazonS3> NewImagesClient()
        {
            var accessKeySSMKeyName = Environment.GetEnvironmentVariable("SSM_SiteGenerator_AccessKey");
            var secretKeySSMKeyName = Environment.GetEnvironmentVariable("SSM_SiteGenerator_SecretKey");

            var ssm = new AmazonSimpleSystemsManagementClient();

            var parameters = await ssm.GetParametersAsync(new GetParametersRequest
            {
                Names = new List<string> {accessKeySSMKeyName, secretKeySSMKeyName},
                WithDecryption = true
            });

            var accessKeyParameter = parameters.Parameters.Single(x => x.Name == accessKeySSMKeyName);
            var secretKeyParameter = parameters.Parameters.Single(x => x.Name == secretKeySSMKeyName);

            var credentials = new BasicAWSCredentials(accessKeyParameter.Value, secretKeyParameter.Value);

            return new AmazonS3Client(credentials);
        }

        public SiteGeneratorFunction(IAmazonDynamoDB dynamoDb, IAmazonS3 s3Site, IAmazonS3 s3Images)
        {
            _dynamoDb = dynamoDb;
            _s3Site = s3Site;
            _s3Images = s3Images;
        }

        public async Task Handle()
        {
            var expirationTime = DateTime.UtcNow.Add(ExpirationPeriod);

            var provider = new DynamoDBImageProvider(_dynamoDb, _s3Images, BucketNames.Images, expirationTime);
            var generator = new SiteGenerator(provider);

            var site = await generator.GenerateAsync();

            var storer = new S3SiteStorer(_s3Site, BucketNames.Site, expirationTime);
            await storer.StoreAsync(site);
        }
    }

    public class DynamoDBImageProvider : IImageProvider
    {
        private readonly IAmazonS3 _s3;
        private readonly string _bucket;
        private readonly DateTime _expirationTime;

        private readonly Table _photographs;

        public DynamoDBImageProvider(IAmazonDynamoDB dynamoDb, IAmazonS3 s3, string bucket, DateTime expirationTime)
        {
            _s3 = s3;
            _bucket = bucket;
            _expirationTime = expirationTime;

            _photographs = Table.LoadTable(dynamoDb, TableNames.Photograph);
        }

        public async Task<IEnumerable<PhotographViewModel>> GetPrimaryPhotographsAsync()
        {
            var expr = new Expression {ExpressionStatement = "attribute_exists(#layout) AND NOT attribute_type(#layout, :null)"};
            expr.ExpressionAttributeNames.Add("#layout", "layout");
            expr.ExpressionAttributeValues.Add(":null", new Primitive("NULL"));
           
            var search = _photographs.Scan(expr);
            
            var documents = await search.GetAllAsync();
            return documents
                .Select(PhotographSerialization.FromDocument)
                .OrderBy(x => x.Layout.Order)
                .Select(ToViewModel);
        }

        private PhotographViewModel ToViewModel(Photograph photograph)
        {
            string thumbnailUrl = null;

            var thumbnail = photograph.Images.FirstOrDefault(x => x.Type == ImageType.Thumbnail);
            if (thumbnail != null)
            {
                thumbnailUrl = _s3.GetPreSignedURL(new GetPreSignedUrlRequest
                {
                    BucketName = _bucket,
                    Key = thumbnail.ObjectKey,

                    Expires = _expirationTime,
                });
            }

            return new PhotographViewModel(photograph, thumbnailUrl);
        }
    }

    public class S3SiteStorer
    {
        private readonly IAmazonS3 _s3;
        private readonly string _bucket;
        private readonly DateTime _expirationTime;

        public S3SiteStorer(IAmazonS3 s3, string bucket, DateTime expirationTime)
        {
            _s3 = s3;
            _bucket = bucket;
            _expirationTime = expirationTime;
        }

        public async Task StoreAsync(Site site)
        {
            foreach (var file in site.Files)
            {
                await StoreFile(file);
            }
        }

        private async Task StoreFile(File file)
        {
            var content = await file.GenerateAsync();

            // TODO is there a nicer way of doing this?
            using (var ms = new MemoryStream())
            {
                await ms.WriteAsync(content);

                var request = new PutObjectRequest
                {
                    BucketName = _bucket,
                    Key = file.Name,

                    ContentType = file.ContentType,
                    Headers = { ExpiresUtc = _expirationTime },

                    InputStream = ms,
                };

                await _s3.PutObjectAsync(request);
            }
        }
    }
}
