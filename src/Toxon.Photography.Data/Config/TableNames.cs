using System;

namespace Toxon.Photography.Data.Config;

public class TableNames
{
    public static readonly string Photograph = Environment.GetEnvironmentVariable("PHOTOGRAPH_TABLE") ?? throw new InvalidOperationException("PHOTOGRAPH_TABLE environment variable was not set.");
    public static readonly string Story = Environment.GetEnvironmentVariable("STORY_TABLE") ?? throw new InvalidOperationException("STORY_TABLE environment variable was not set.");
}