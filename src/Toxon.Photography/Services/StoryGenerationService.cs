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
        var optionDatePrompt = "";
        var imagePrompt = " - don't worry about available images yet.";
        
        if (story is { StartDate: not null, EndDate: not null })
        {
            optionDatePrompt = $", the user has provided start date ({story.StartDate:yyyy-MM-dd}) and end date ({story.EndDate:yyyy-MM-dd}) to help identify date ranges";

            var (_, availablePhotographsJson) = await GetImagesInDateRangeAsync(story.StartDate, story.EndDate);
            imagePrompt = $", consider available images from this date range to help with sectioning, but remember that suggestions can be given later to upload additional images: {availablePhotographsJson}";
        }
        
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

                       Focus on the best way to tell the story{{imagePrompt}}

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
            ModelId = "amazon.nova-pro-v1:0",
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
        var (photographs, availablePhotographsJson) = await GetImagesInDateRangeAsync(section.DateRange.Start, section.DateRange.End);

        var prompt = $$"""
                        # Travel Journal Content Block Creator
                        
                        ## Task Description
                        You are a professional travel content editor tasked with creating engaging, well-structured content blocks for a travel journal section. Your goal is to transform raw journal entries into a polished narrative while preserving the authentic voice and key details.
                        
                        ## Input Materials
                        You will work with:
                        1. A planned section structure (title, date range, summary)
                        2. Raw journal text entries for the whole trip, which may include details outside the section date range for context
                        3. Available images from this time period (if any)
                        
                        ## Section Information
                        - Title: "{{section.Title}}"
                        - Date range: "{{section.DateRange.Start:yyyy-MM-dd}}" to "{{section.DateRange.End:yyyy-MM-dd}}"
                        - Summary: "{{section.Summary}}"
                        - Theme: "{{section.Theme}}"

                        ## Journal Entries
                        {{story.Journal}}

                        ## Available Images
                        {{availablePhotographsJson}}

                        ## Instructions
                        Create a series of content blocks that tell the story of this travel segment in an engaging, authentic way. Follow these guidelines:
                        
                        1. **Content Creation Guidelines:**
                           - Write narrative text that improves flow while preserving authentic details and voice
                           - Use first-person perspective to maintain the personal journal feel
                           - Vary paragraph length to create an engaging rhythm
                           - Synthesize information from the journal entries for the specified date range
                           - You may reference previous events for context when relevant, but avoid anything past the section end date
                        
                        2. **Visual Content Guidelines:**
                           - Use available images where they naturally enhance the narrative
                           - Each image must reference an actual photographId from the available images list
                           - Create suggestion blocks for places where additional images would enhance the narrative
                           - When possible, use text from the journal as image captions
                        
                        3. **Structure Guidelines:**
                           - Alternate between text and visual content for an engaging rhythm
                           - Only create "text", "image", and "suggestion" block types
                           - Ensure the blocks flow logically and tell a cohesive story
                        
                        ## Output Format
                        Provide your response as a valid JSON array of content blocks. Each block should have the appropriate structure based on its type:
                        
                        ```json
                        [
                          {"$type": "text",
                            "content": "Narrative text paragraph here..."},
                          {"$type": "image",
                            "photographId": "<guid from list of available images>",
                            "caption": "Caption text here (optional)"},
                          {"$type": "suggestion",
                            "prompt": "Suggestion for additional image to upload"}
                        ]
                        ```
                        
                        Create a concise, engaging narrative that captures the essence of this travel experience while maintaining the authentic voice from the journal entries.
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
            ModelId = "amazon.nova-pro-v1:0",
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

    private async Task<(List<Photograph> photographs, string availablePhotographsJson)> GetImagesInDateRangeAsync(DateTime? startDate, DateTime? endDate)
    {
        var filter = new ScanFilter();
        filter.AddCondition(PhotographSerialization.Fields.CaptureTime, ScanOperator.Between, startDate?.Date, endDate?.Date.AddDays(1));
        
        var photographDocuments = await _photographTable.Scan(filter).GetAllAsync();
        var photographs = photographDocuments.Select(PhotographSerialization.FromDocument).ToList();
        
        var availablePhotographsJson = JsonSerializer.Serialize(photographs.Select(x => new { x.Id, x.Title, x.CaptureTime }));
        
        return (photographs, availablePhotographsJson);
    }
}
