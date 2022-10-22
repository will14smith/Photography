using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.SNSEvents;

namespace Toxon.Photography.ThumbnailProcessor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var rawInput = Environment.GetEnvironmentVariable("INPUT");
            SNSEvent input;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(rawInput)))
            {
                var serializer = new Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer();
                input = serializer.Deserialize<SNSEvent>(stream);
            }

            var processor = new ThumbnailProcessorFunction();
            await processor.Handle(input);
        }
    }
}
