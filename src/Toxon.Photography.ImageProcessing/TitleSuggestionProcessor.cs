using System.Text.Json.Nodes;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Toxon.Photography.Data;
using Toxon.Photography.Data.Config;

namespace Toxon.Photography.ImageProcessing;

public class TitleSuggestionProcessor(IAmazonBedrockRuntime bedrock)
{
    public async Task<IReadOnlyCollection<string>> SuggestTitlesAsync(Photograph photograph)
    {
        var biggestThumbnail = photograph.Images.Where(x => x.Type == ImageType.Thumbnail).MaxBy(x => x.Width);
        if (biggestThumbnail == null)
        {
            throw new InvalidOperationException("Photograph has no thumbnails for title suggestion.");
        }

        var photographUri = $"s3://{BucketNames.Images}/{biggestThumbnail.ObjectKey}";

        var buffer = $$"""
                       {
                          "inferenceConfig": {
                              "maxTokens": 512,
                              "stopSequences": ["```"],
                              "temperature": 0.7,
                              "topP": 0.90
                          },
                          "messages": [
                              {
                                  "role": "user",
                                  "content": [
                                      { "text": "Generate up to 5 concise, descriptive titles for this photograph (maximum 6 words).\nEach title should capture the key subject, mood, or visual elements that make the image distinctive.\nUse specific, evocative language rather than generic descriptions.\nFormat: Title Case without quotation marks\nResponse should be formatted as a JSON array with a title string as each element e.g. `[\"Title1\", \"Title2\", \"Title3\"]`. Please generate only the JSON output. DO NOT provide any preamble." },
                                      { "image": { "format": "jpeg", "source": { "s3Location": { "uri": "{{photographUri}}" } } } } 
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
        var responseArray = JsonNode.Parse(responseText).AsArray();

        var titles = new List<string>();
        foreach (var item in responseArray)
        {
            if (item == null)
            {
                continue;
            }
            
            var title = item.AsValue();
            titles.Add(title.ToString());
        }
        
        return titles;
    }
}