using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.EventBridge;
using Microsoft.AspNetCore.Mvc;
using Toxon.Photography.Data;
using Toxon.Photography.Data.Config;
using Toxon.Photography.Generation.Extensions;
using Toxon.Photography.Models;

namespace Toxon.Photography.Controllers;

[ApiController]
[Route("[controller]")]
public class PhotographController(IAmazonDynamoDB dynamoDb, IAmazonEventBridge eventBridge) : ControllerBase
{
    private readonly Table _photographTable = Table.LoadTable(dynamoDb, TableNames.Photograph);

    [HttpGet]
    public async Task<IEnumerable<Photograph>> List()
    {
        var search = _photographTable.Scan(new ScanFilter());
        
        var documents = await search.GetAllAsync();

        return documents.Select(PhotographSerialization.FromDocument).OrderByDescending(x => x.UploadTime);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<Photograph>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id)
    {
        var document = await _photographTable.GetItemAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        var model = PhotographSerialization.FromDocument(document);
        return Ok(model);
    }

    [HttpPost]
    [ProducesResponseType<Photograph>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] PhotographCreateModel model)
    {
        var photograph = model.ToPhotograph();

        await _photographTable.PutItemAsync(PhotographSerialization.ToDocument(photograph));
        
        await PhotographEvent.SendAsync(eventBridge, "create", photograph);
        
        return Created($"{photograph.Id}", photograph);
    }
    
    [HttpPut("{id:guid}")]
    [ProducesResponseType<Photograph>(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Edit(Guid id, [FromBody] PhotographyEditModel model)
    {
        var updateDocument = model.ToDocument();

        var document = await _photographTable.UpdateItemAsync(updateDocument, id, new UpdateItemOperationConfig { ReturnValues = ReturnValues.AllNewAttributes });
        var photograph = PhotographSerialization.FromDocument(document);
        
        await PhotographEvent.SendAsync(eventBridge, "update", photograph);

        return Accepted(photograph);
    }
}