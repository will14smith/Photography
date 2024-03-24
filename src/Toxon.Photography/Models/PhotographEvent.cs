using System.Text.Json;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Toxon.Photography.Data;

namespace Toxon.Photography.Models;

public class PhotographEvent
{
    public Photograph Photograph { get; set; }

    public async Task SendAsync(IAmazonEventBridge eventBridge, string action)
    {
        await eventBridge.PutEventsAsync(new PutEventsRequest
        {
            Entries = [
                new PutEventsRequestEntry
                {
                    Detail = JsonSerializer.Serialize(this),
                    DetailType = $"photograph.{action}",
                    Source = "photography",
                }
            ]
        });
    }

    public static async Task SendAsync(IAmazonEventBridge eventBridge, string action, Photograph photograph)
    {
        await new PhotographEvent { Photograph = photograph }.SendAsync(eventBridge, action);
    }
}