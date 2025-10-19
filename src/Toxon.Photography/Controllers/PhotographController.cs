using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Microsoft.AspNetCore.Mvc;
using Toxon.Photography.Data;
using Toxon.Photography.Generation.Extensions;
using Toxon.Photography.Models;

namespace Toxon.Photography.Controllers;

[ApiController]
[Route("[controller]")]
public class PhotographController(IAmazonDynamoDB dynamoDb, IAmazonEventBridge eventBridge) : ControllerBase
{
    private readonly ITable _photographTable = PhotographTable.Create(dynamoDb);

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
        
        await SendEventAsync("create", photograph);
        
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
        
        await SendEventAsync("update", photograph);

        return Accepted(photograph);
    }

    public async Task SendEventAsync(string action, Photograph photograph)
    {
        var eventDetail = new PhotographEvent { Photograph = photograph };
        
        await eventBridge.PutEventsAsync(new PutEventsRequest
        {
            Entries = [
                new PutEventsRequestEntry
                {
                    Detail = JsonSerializer.Serialize(eventDetail),
                    DetailType = $"photograph.{action}",
                    Source = "photography",
                }
            ]
        });
    }
}