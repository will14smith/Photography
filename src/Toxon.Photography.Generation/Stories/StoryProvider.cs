using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Toxon.Photography.Data;

namespace Toxon.Photography.Generation.Stories;

public class StoryProvider(IAmazonDynamoDB dynamoDb)
{
    private readonly ITable _storyTable = StoryTable.Create(dynamoDb);
    
    public async Task<IEnumerable<Story>> GetStoriesAsync()
    {
        var documents = await _storyTable.Scan(new ScanFilter()).GetAllAsync();
        return documents.Select(StorySerialization.FromDocument);
    }
}