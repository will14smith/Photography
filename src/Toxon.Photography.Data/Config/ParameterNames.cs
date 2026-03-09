using System;

namespace Toxon.Photography.Data.Config;

public class ParameterNames
{
    public static string CloudFrontImageBaseUrl => Environment.GetEnvironmentVariable("CLOUDFRONT_IMAGE_BASE_URL") ?? throw new InvalidOperationException("CLOUDFRONT_IMAGE_BASE_URL environment variable was not set.");
    public static string CloudFrontImageKeyPairId => Environment.GetEnvironmentVariable("CLOUDFRONT_IMAGE_KEY_PAIR_ID") ?? throw new InvalidOperationException("CLOUDFRONT_IMAGE_KEY_PAIR_ID environment variable was not set.");
    public static string CloudFrontPrivateKeyPath => Environment.GetEnvironmentVariable("CLOUDFRONT_PRIVATE_KEY_SSM_PATH") ?? throw new InvalidOperationException("CLOUDFRONT_PRIVATE_KEY_SSM_PATH environment variable was not set.");
}