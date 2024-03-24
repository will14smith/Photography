using System;

namespace Toxon.Photography.Data.Config;

public class TableNames
{
    public static readonly string Photograph = Environment.GetEnvironmentVariable("PHOTOGRAPH_TABLE") ?? throw new InvalidOperationException("PHOTOGRAPH_TABLE environment variable was not set.");
}