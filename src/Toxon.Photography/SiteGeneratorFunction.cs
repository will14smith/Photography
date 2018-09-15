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
            var provider = new DynamoDBImageProvider(_dynamoDb, _s3, BucketNames.Images);
            var generator = new SiteGenerator(provider);

            var site = await generator.GenerateAsync();

            var storer = new S3SiteStorer(_s3, BucketNames.Site);
            await storer.StoreAsync(site);
        }
    }

    public class DynamoDBImageProvider : IImageProvider
    {
        private readonly IAmazonS3 _s3;
        private readonly string _bucket;

        private readonly Table _photographs;

        public DynamoDBImageProvider(IAmazonDynamoDB dynamoDb, IAmazonS3 s3, string bucket)
        {
            _s3 = s3;
            _bucket = bucket;

            _photographs = Table.LoadTable(dynamoDb, TableNames.Photograph);
        }

        public async Task<IEnumerable<PhotographViewModel>> GetPrimaryPhotographsAsync()
        {
            var filter = new ScanFilter();
            filter.AddCondition(PhotographSerialization.Fields.LayoutPosition, ScanOperator.IsNotNull);
            var search = _photographs.Scan(filter);

            var documents = await search.GetAllAsync();
            return documents
                .Select(PhotographSerialization.FromDocument)
                .OrderBy(x => x.LayoutPosition)
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

                    Expires = DateTime.UtcNow.AddDays(2),
                });
            }

            return new PhotographViewModel(photograph, thumbnailUrl);
        }
    }

    public class S3SiteStorer
    {
        private readonly IAmazonS3 _s3;
        private readonly string _bucket;

        public S3SiteStorer(IAmazonS3 s3, string bucket)
        {
            _s3 = s3;
            _bucket = bucket;
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

                await _s3.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = _bucket,
                    Key = file.Name,
                    ContentType = file.ContentType,

                    InputStream = ms
                });
            }
        }
    }
}
