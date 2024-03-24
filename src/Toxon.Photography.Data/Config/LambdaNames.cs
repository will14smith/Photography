using System;

namespace Toxon.Photography.Data.Config;

public class LambdaNames
{
    public static string SiteGenerator => Environment.GetEnvironmentVariable("SITE_GENERATOR_LAMBDA") ?? throw new InvalidOperationException("SITE_GENERATOR_LAMBDA environment variable was not set.");

}