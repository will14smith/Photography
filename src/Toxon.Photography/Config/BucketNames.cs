using System;

namespace Toxon.Photography.Config
{
    public static class BucketNames
    {
        public static readonly string Images = Environment.GetEnvironmentVariable("IMAGE_BUCKET");
        public static readonly string Site = Environment.GetEnvironmentVariable("SITE_BUCKET");
    }
}