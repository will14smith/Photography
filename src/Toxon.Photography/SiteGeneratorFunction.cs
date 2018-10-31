using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.S3;
using Amazon.S3.Model;
using Toxon.Photography.Config;
using Toxon.Photography.Data;
using Toxon.Photography.Generation;
using Toxon.Photography.Generation.Models;
using File = Toxon.Photography.Generation.Models.File;

namespace Toxon.Photography
{
    public class SiteGeneratorFunction
    {
        private static readonly TimeSpan ExpirationPeriod = TimeSpan.FromDays(2);

        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly IAmazonS3 _s3;

        public SiteGeneratorFunction()
            : this(new AmazonDynamoDBClient(), new AmazonS3Client())
        {
        }
        public SiteGeneratorFunction(IAmazonDynamoDB dynamoDb, IAmazonS3 s3)
        {
            _dynamoDb = dynamoDb;
            _s3 = s3;
        }

        public async Task Handle()
        {
            var expirationTime = DateTime.UtcNow.Add(ExpirationPeriod);

            var provider = new DynamoDBImageProvider(_dynamoDb, _s3, BucketNames.Images, expirationTime);
            var generator = new SiteGenerator(provider);

            var site = await generator.GenerateAsync();

            var storer = new S3SiteStorer(_s3, BucketNames.Site, expirationTime);
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
            var filter = new ScanFilter();
            filter.AddCondition(PhotographSerialization.Fields.Layout, ScanOperator.IsNotNull);
            var search = _photographs.Scan(filter);

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

                    InputStream = ms
                };

                request.Metadata.Add("Expires", _expirationTime.ToString("R"));

                await _s3.PutObjectAsync(request);
            }
        }
    }
}
