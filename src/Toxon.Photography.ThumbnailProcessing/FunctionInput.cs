using System.Text.Json.Serialization;
using Toxon.Photography.Data;

namespace Toxon.Photography.ThumbnailProcessing;

public class FunctionInput
{
    [JsonPropertyName("detail-type")]
    public string? DetailType { get; set; }
    [JsonPropertyName("detail")]
    public PhotographEvent? Detail { get; set; }
}