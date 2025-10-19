using Amazon.S3;
using Amazon.S3.Model;
using Toxon.Photography.Data.Config;

namespace Toxon.Photography.Generation;

public class S3SiteStorer
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket = BucketNames.Site;

    public S3SiteStorer(IAmazonS3 s3)
    {
        _s3 = s3;
    }

    public async Task StoreAsync(Site site)
    {
        foreach (var file in site.Files)
        {
            await StoreFile(file);
        }
    }

    private async Task StoreFile(Site.File file)
    {
        using var ms = new MemoryStream();
        await ms.WriteAsync(file.Content);

        var request = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = file.Name,

            ContentType = file.ContentType,
            Headers = { Expires = DateTime.UtcNow.Add(SiteGeneratorLambda.ExpirationPeriod) },

            InputStream = ms,
        };

        await _s3.PutObjectAsync(request);
    }
}
