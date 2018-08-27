using System;

namespace Toxon.Photography.Config
{
    public static class TopicArns
    {
        public static readonly string ImageProcessor = Environment.GetEnvironmentVariable("IMAGE_PROCESSOR_TOPIC_ARN");
    }
}
