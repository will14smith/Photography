using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Mvc;
using Toxon.Photography.Data;
using Toxon.Photography.Data.Config;
using Toxon.Photography.Generation.Extensions;
using Toxon.Photography.Models;

namespace Toxon.Photography.Controllers;

[ApiController]
[Route("layout")]
public class LayoutController(IAmazonDynamoDB dynamoDb) : ControllerBase
{
    private readonly Table _photographTable = Table.LoadTable(dynamoDb, TableNames.Photograph);

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Save([FromBody] IReadOnlyDictionary<Guid, LayoutModel?> model)
    {
        var allIds = await GetAllIds();

        foreach (var id in allIds)
        {
            var layout = model.GetValueOrDefault(id);

            await SetLayoutForPhotograph(id, layout);
        }
        
        return Accepted();
    }
    
    private async Task<IReadOnlyCollection<Guid>> GetAllIds()
    {
        var search = _photographTable.Scan(new ScanOperationConfig
        {
            AttributesToGet = [PhotographSerialization.Fields.Id],
            Select = SelectValues.SpecificAttributes,
        });

        var documents = await search.GetAllAsync();

        return documents.Select(x => x[PhotographSerialization.Fields.Id].AsGuid()).ToList();
    }

    private async Task SetLayoutForPhotograph(Guid id, LayoutModel? model)
    {
        var layout = model is not null ? new Layout
        {
            Order = model.Order,

            Width = model.Width,
            Height = model.Height,
        } : null;

        var document = new Document
        {
            [PhotographSerialization.Fields.Layout] = LayoutSerialization.ToDocument(layout),
        };

        await _photographTable.UpdateItemAsync(document, id, new UpdateItemOperationConfig { ReturnValues = ReturnValues.None });
    }
}