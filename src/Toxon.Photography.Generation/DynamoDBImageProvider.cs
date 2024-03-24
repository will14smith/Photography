using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.DependencyInjection;
using Toxon.Photography.Data;
using Toxon.Photography.Data.Config;
using Toxon.Photography.Generation.Extensions;
using Toxon.Photography.Generation.Models;

namespace Toxon.Photography.Generation;

public class DynamoDbImageProvider(IAmazonDynamoDB dynamoDb, [FromKeyedServices(DynamoDbImageProvider.S3ClientInjectionKey)] IAmazonS3 s3)
{
    internal const string S3ClientInjectionKey = $"{nameof(DynamoDbImageProvider)}.{nameof(IAmazonS3)}";
    
    private readonly Table _photographs = Table.LoadTable(dynamoDb, TableNames.Photograph);

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

        var thumbnail = photograph.Images.LastOrDefault(x => x.Type == ImageType.Thumbnail);
        if (thumbnail != null)
        {
            thumbnailUrl = s3.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = BucketNames.Images,
                Key = thumbnail.ObjectKey,

                Expires = DateTime.UtcNow.Add(SiteGeneratorLambda.ExpirationPeriod),
            });
        }

        return new PhotographViewModel(photograph, thumbnailUrl);
    }
}