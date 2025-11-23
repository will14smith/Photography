using System.Text.Json;
using System.Text.Json.Nodes;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Toxon.Photography.Data;
using Toxon.Photography.Models;
using ImageBlock = Toxon.Photography.Data.ImageBlock;

namespace Toxon.Photography.Services;

public class StoryGenerationService(IAmazonBedrockRuntime bedrock, IAmazonDynamoDB dynamoDb) : IStoryGenerationService
{
    private readonly ITable _photographTable = PhotographTable.Create(dynamoDb);
    
    public async Task<StoryAnalysis> AnalyseStoryForSectionsAsync(Story story)
    {
        var optionDatePrompt = story is { StartDate: not null, EndDate: not null }
            ? $", the user has provided start date ({story.StartDate:yyyy-MM-dd}) and end date ({story.EndDate:yyyy-MM-dd}) to help identify date ranges."
            : "";
        
        var prompt = $$"""
                       Analyze this travel journal and create a section outline for a photo story.

                       First, extract:
                       - Date range covered (start and end dates){{optionDatePrompt}}
                       - Key locations or themes

                       Then create 3-7 sections based on natural narrative breaks. Sections should be substantial - think of them as chapters, not individual days.
                       Generally travel to/from a place should be grouped into the same section as the actual place, with the section focus being on the place rather than the travel. Consider grouping by:
                       - Major locations or cities (e.g., "Three Days in Kyoto", including the travel to Kyoto)
                       - Significant experiences or themes (e.g., "Temple Pilgrimage Across Kansai")
                       - Natural narrative arcs (e.g., "From Rural Temples to Urban Nights")
                       
                       For each section:
                       - Write a compelling title (4-8 words)
                       - Specify the date range or time period
                       - Write a 1-2 sentence summary covering the key experiences
                       - Identify the narrative theme
                       - If a grammatical perspective is needed then prefer the first person

                       Focus on the best way to tell the story - don't worry about available images yet.

                       Journal:
                       {{story.Journal}}

                       Output JSON:
                       {
                         "dateRange": {
                           "start": "2024-10-15",
                           "end": "2024-10-21"
                         },
                         "metadata": {
                           "primaryLocations": ["Kyoto", "Nara", "Osaka"],
                           "overallTheme": "Cultural exploration and temple visits"
                         },
                         "sections": [
                           {
                             "title": "Kyoto: Temples, Gardens, and Geishas",
                             "dateRange": {"start": "2024-10-15", "end": "2024-10-18"},
                             "summary": "Four days exploring Kyoto's ancient temples from Fushimi Inari's torii gates to Kinkaku-ji's golden pavilion, wandering bamboo groves in Arashiyama, evening encounters in Gion, and discovering hidden craft workshops in quiet neighborhoods",
                             "theme": "Cultural Immersion and Spiritual Exploration"
                           },
                           {
                             "title": "Nara's Sacred Deer and Ancient Wonders",
                             "dateRange": {"start": "2024-10-18", "end": "2024-10-19"},
                             "summary": "Day trips to Nara encountering sacred deer in the park, exploring the massive Todai-ji temple with its giant Buddha, wandering lantern-lined paths at Kasuga Taisha, and discovering quieter temples in the countryside",
                             "theme": "Wildlife and Ancient Heritage"
                           },
                           {
                             "title": "Osaka: From Serenity to Neon",
                             "dateRange": {"start": "2024-10-20", "end": "2024-10-21"},
                             "summary": "Final days transitioning from temple tranquility to Osaka's electric energy - Osaka Castle's gardens, Dotonbori's neon-lit street food chaos, and last morning reflections before departure",
                             "theme": "Urban Contrast and Departure"
                           }
                         ]
                       }
                       """;
        
        var buffer = $$"""
                       {
                           "inferenceConfig": {
                               "maxTokens": 4096,
                               "stopSequences": ["```"],
                               "temperature": 0.7,
                               "topP": 0.90
                           },
                          "system": [{
                            "text": "You are a travel story editor. Your task is to transform travel journals into engaging photo stories by organizing content into coherent sections, selecting relevant images, and refining the narrative while preserving the author's authentic voice and experiences."
                          }],
                          "messages": [
                              {
                                  "role": "user",
                                  "content": [
                                      { "text": {{JsonSerializer.Serialize(prompt)}} }
                                  ]
                              },
                              {
                                  "role": "assistant",
                                  "content": [{"text": "Here is the JSON response: ```json"}]
                              }
                          ]
                       }
                       """;
        
        using var body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(buffer));
        var request = new InvokeModelRequest
        {
            ModelId = "amazon.nova-lite-v1:0",
            Body = body,
            ContentType = "application/json",
        };

        var response = await bedrock.InvokeModelAsync(request);
        var responseBody = await JsonNode.ParseAsync(response.Body);
        if (responseBody == null)
        {
            throw new InvalidOperationException("Failed to parse response from Bedrock.");
        }
        var responseText = responseBody["output"]["message"]["content"][0]["text"].AsValue().ToString().Trim('`');

        return JsonSerializer.Deserialize<StoryAnalysis>(responseText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new InvalidOperationException("Failed to deserialize story analysis.");
    }
    
    public async Task<IEnumerable<Block>> GenerateSectionBlocksAsync(Story story, StorySectionAnalysis section)
    {
        var filter = new ScanFilter();
        filter.AddCondition(PhotographSerialization.Fields.CaptureTime, ScanOperator.Between, section.DateRange.Start?.Date, section.DateRange.End?.Date.AddDays(1));
        var photographDocuments = await _photographTable.Scan(filter).GetAllAsync();
        var photographs = photographDocuments.Select(PhotographSerialization.FromDocument).ToList();
        var availablePhotographsJson = JsonSerializer.Serialize(photographs.Select(x => new { x.Id, x.Title, x.CaptureTime }));
        
        var prompt = $$"""
                       Create detailed content blocks for this section. You'll work with:
                       1. The planned section structure
                       2. Relevant journal text
                       3. Available images from this time period
                       
                       Section outline:
                       - Title: "{{section.Title}}"
                       - Date range: "{{section.DateRange.Start:yyyy-MM-dd}}" to "{{section.DateRange.End:yyyy-MM-dd}}"
                       - Summary: "{{section.Summary}}"
                       - Theme: "{{section.Theme}}"
                       
                       Journal (full, will need filtered for the section date range, but can reference previous events for context):
                       {{story.Journal}}
                       
                       Available images in section date range: {{availablePhotographsJson}}
                       
                       Create blocks that:
                       - Use available images where they naturally enhance the narrative, the photographId is a foreign key so MUST reference the images above, having no images is acceptable
                       - Add suggestion blocks for places where an additional image could be uploaded help the narrative and there is no suitable image available
                       - Where possible text synthesised from the journal should be used as the caption for images
                       - Write narrative text that improves flow while preserving authentic details and voice, generally using the first person
                       - Alternate between text and visual content for engaging rhythm
                       - Only create "text", "image", and "suggestion" block types
                       
                       Output JSON:
                       [
                         {
                           "$type": "text",
                           "content": "I set my alarm for 05:30, determined to experience Fushimi Inari before the tour groups arrived. The early train was nearly empty, filled only with a few dedicated photographers and fellow early risers. As I approached the shrine, the first torii gates glowed in the predawn light."
                         },
                         {
                           "$type": "image",
                           "photographId": "<guid from list of available images>",
                           "caption": "The tunnel of vermillion gates seemed to stretch endlessly up the mountainside"
                         },
                         {
                           "$type": "text",
                           "content": "The torii gates formed an otherworldly tunnel, each one donated by businesses and families over centuries. The path wound upward through the forest, occasionally opening to reveal smaller shrines and fox statues. By the time I reached the summit, my legs were burning, but the panoramic view of Kyoto spreading out below made every step worthwhile."
                         },
                         {
                           "$type": "suggestion",
                           "prompt": "Consider including a photograph of the monk who explained the temple's water purification rituals at the Otowa Waterfall"
                         },
                         {
                           "$type": "text",
                           "content": "After descending and grabbing a quick breakfast, I made my way to Kiyomizu-dera. The temple's famous wooden stage, built without a single nail, projects out from the hillside like a ship's prow. Standing on it, surrounded by other visitors, I could see why this view has captivated people for over a thousand years."
                         },
                         {
                           "$type": "image",
                           "photographId": "<guid from list of available images>",
                         }
                       ]
                       """;
        
        var buffer = $$"""
                       {
                           "inferenceConfig": {
                               "maxTokens": 4096,
                               "stopSequences": ["```"],
                               "temperature": 0.7,
                               "topP": 0.90
                           },
                          "system": [{
                            "text": "You are a travel story editor. Your task is to transform travel journals into engaging photo stories by organizing content into coherent sections, selecting relevant images, and refining the narrative while preserving the author's authentic voice and experiences."
                          }],
                          "messages": [
                              {
                                  "role": "user",
                                  "content": [
                                      { "text": {{JsonSerializer.Serialize(prompt)}} }
                                  ]
                              },
                              {
                                  "role": "assistant",
                                  "content": [{"text": "Here is the JSON response: ```json"}]
                              }
                          ]
                       }
                       """;
        
        using var body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(buffer));
        var request = new InvokeModelRequest
        {
            ModelId = "amazon.nova-lite-v1:0",
            Body = body,
            ContentType = "application/json",
        };

        var response = await bedrock.InvokeModelAsync(request);
        var responseBody = await JsonNode.ParseAsync(response.Body);
        if (responseBody == null)
        {
            throw new InvalidOperationException("Failed to parse response from Bedrock.");
        }
        var responseText = responseBody["output"]["message"]["content"][0]["text"].AsValue().ToString().Trim('`');

        var blocks = JsonSerializer.Deserialize<IReadOnlyList<Block>>(responseText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new InvalidOperationException("Failed to deserialize story analysis.");
        
        var photographIds = photographs.Select(p => p.Id.ToString()).ToHashSet();
        return blocks.Select(block =>
        {
            if (block is not ImageBlock imageBlock)
            {
                return block;
            }

            // the LLM likes to invent photograph IDs that don't exist, so replace with suggestion blocks
            return imageBlock.PhotographId is null || !photographIds.Contains(imageBlock.PhotographId)
                ? new SuggestionBlock { Prompt = imageBlock.Caption ?? "Consider adding an image" }
                : block;
        });
    }
}
