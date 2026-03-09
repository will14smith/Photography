using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Toxon.Photography.Data;
using Toxon.Photography.Generation.Models;

namespace Toxon.Photography.Generation;

public class DynamoDbImageProvider(IAmazonDynamoDB dynamoDb, CloudFrontSignedUrlGenerator signedUrlGenerator)
{
    private readonly ITable _photographs = PhotographTable.Create(dynamoDb);

    public async Task<IEnumerable<PhotographWithLayoutViewModel>> GetPrimaryPhotographsAsync()
    {
        var expr = new Expression {ExpressionStatement = "attribute_exists(#layout) AND NOT attribute_type(#layout, :null)"};
        expr.ExpressionAttributeNames.Add("#layout", PhotographSerialization.Fields.Layout);
        expr.ExpressionAttributeValues.Add(":null", new Primitive("NULL"));
           
        var search = _photographs.Scan(expr);
            
        var documents = await search.GetAllAsync();
        return documents
            .Select(PhotographSerialization.FromDocument)
            // filter ensures layout is not null
            .OrderBy(x => x.Layout!.Order)
            .Select(ToViewModelWithLayout);
    }

    public async Task<IEnumerable<PhotographViewModel>> GetPhotographsByIdAsync(IReadOnlyCollection<string> ids)
    {
        var batchGet = _photographs.CreateBatchGet();
        foreach (var id in ids)
        {
            batchGet.AddKey(id);
        }

        await batchGet.ExecuteAsync();
        
        return batchGet.Results
            .Select(PhotographSerialization.FromDocument)
            .Select(ToViewModel);
    }

    private PhotographWithLayoutViewModel ToViewModelWithLayout(Photograph photograph) => new(photograph, photograph.Layout!, GetThumbnailUrl(photograph));
    private PhotographViewModel ToViewModel(Photograph photograph) => new(photograph, GetThumbnailUrl(photograph));

    private string? GetThumbnailUrl(Photograph photograph)
    {
        string? thumbnailUrl = null;

        var thumbnail = photograph.Images.LastOrDefault(x => x.Type == ImageType.Thumbnail);
        if (thumbnail != null)
        {
            thumbnailUrl = signedUrlGenerator.GetSignedUrl(thumbnail.ObjectKey);
        }

        return thumbnailUrl;
    }

}