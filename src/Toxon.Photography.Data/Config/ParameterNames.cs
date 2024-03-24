using System;

namespace Toxon.Photography.Data.Config;

public class ParameterNames
{
    public static string SigningAccessKeyPath => Environment.GetEnvironmentVariable("SITE_GENERATOR_ACCESS_KEY_SSM_PATH") ?? throw new InvalidOperationException("SITE_GENERATOR_ACCESS_KEY_SSM_PATH environment variable was not set.");
    public static string SigningSecretKeyPath => Environment.GetEnvironmentVariable("SITE_GENERATOR_SECRET_KEY_SSM_PATH") ?? throw new InvalidOperationException("SITE_GENERATOR_SECRET_KEY_SSM_PATH environment variable was not set.");

}