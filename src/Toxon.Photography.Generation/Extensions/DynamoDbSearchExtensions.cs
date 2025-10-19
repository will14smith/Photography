using Amazon.DynamoDBv2.DocumentModel;

namespace Toxon.Photography.Generation.Extensions;

public static class DynamoDbSearchExtensions
{
    public static async Task<IEnumerable<Document>> GetAllAsync(this ISearch search)
    {
        var documents = new List<Document>();

        do
        {
            documents.AddRange(await search.GetNextSetAsync());
        } while (!search.IsDone);

        return documents;
    }
}
