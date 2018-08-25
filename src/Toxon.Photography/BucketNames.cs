using System;

namespace Toxon.Photography
{
    public static class BucketNames
    {
        public static readonly string Images = Environment.GetEnvironmentVariable("IMAGE_BUCKET");
    }
}