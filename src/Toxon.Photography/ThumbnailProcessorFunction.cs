using System;
using System.Threading.Tasks;
using Amazon.Lambda.SNSEvents;
using Newtonsoft.Json;
using Toxon.Photography.Models;

namespace Toxon.Photography
{
    public class ThumbnailProcessorFunction
    {
        public async Task Handle(SNSEvent snsEvent)
        {
            foreach (var record in snsEvent.Records)
            {
                var rawMessage = record.Sns.Message;
                var message = JsonConvert.DeserializeObject<ImageProcessorMessage>(rawMessage);

                await HandleMessageAsync(message);
            }
        }

        private Task HandleMessageAsync(ImageProcessorMessage message)
        {
            Console.WriteLine("Handling thumbnail processor for: " + message.PhotographId);

            return Task.CompletedTask;
        }
    }
}
