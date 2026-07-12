using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Mvc;
using Toxon.Photography.Data;
using Toxon.Photography.Models;
using Toxon.Photography.Services;

namespace Toxon.Photography.Controllers;

[ApiController]
[Route("/stories")]
public class StoryController(IAmazonDynamoDB dynamoDb, IStoryGenerationService storyGenerationService) : ControllerBase
{
    private readonly ITable _storyTable = StoryTable.Create(dynamoDb);

    [HttpGet]
    public async Task<IEnumerable<Story>> GetAll()
    {
        var documents = await _storyTable.Scan(new ScanFilter()).GetAllAsync();

        return documents.Select(StorySerialization.FromDocument).OrderByDescending(x => x.StartDate);
    }

    [HttpPost]
    public async Task<ActionResult<Story>> Create([FromBody] StoryCreateModel model)
    {
        var story = new Story
        {
            Id = Guid.NewGuid(),
            Title = model.StoryTitle,
            Journal = model.Journal,
            StartDate = model.StartDate ?? DateTime.UtcNow,
            EndDate = model.StartDate ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        };

        await _storyTable.PutItemAsync(StorySerialization.ToDocument(story));
        
        return CreatedAtAction(nameof(Get), new { id = story.Id }, story);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Story>> Get(Guid id)
    {
        var document = await _storyTable.GetItemAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        var model = StorySerialization.FromDocument(document);

        return Ok(model);
    }

    [HttpPost("{id:guid}")]
    public async Task<ActionResult<Story>> Update(Guid id, [FromBody] Story model)
    {
        var updateDocument = StorySerialization.ToDocument(model);

        var document = await _storyTable.UpdateItemAsync(updateDocument, id, new UpdateItemOperationConfig { ReturnValues = ReturnValues.AllNewAttributes });
        var story = StorySerialization.FromDocument(document);

        return Ok(story);
    }
    
    [HttpPost("{id:guid}/analyse")]
    public async Task<ActionResult<StoryAnalysis>> AnalyseSections(Guid id)
    {
        var document = await _storyTable.GetItemAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        var story = StorySerialization.FromDocument(document);

        var sections = await storyGenerationService.AnalyseStoryForSectionsAsync(story);

        return Ok(sections);
    }

    [HttpPost("{id:guid}/section")]
    public async Task<ActionResult<IEnumerable<Block>>> GenerateSection(Guid id, [FromBody] StorySectionAnalysis sectionAnalysis)
    {
        var document = await _storyTable.GetItemAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        var story = StorySerialization.FromDocument(document);

        var blocks = await storyGenerationService.GenerateSectionBlocksAsync(story, sectionAnalysis);

        return Ok(blocks);
    }
}

