using System;

namespace Toxon.Photography.Data.Config;

public static class BucketNames
{
    public static string Images => Environment.GetEnvironmentVariable("IMAGE_BUCKET") ?? throw new InvalidOperationException("IMAGE_BUCKET environment variable was not set.");
    public static string Site => Environment.GetEnvironmentVariable("SITE_BUCKET") ?? throw new InvalidOperationException("SITE_BUCKET environment variable was not set.");
}