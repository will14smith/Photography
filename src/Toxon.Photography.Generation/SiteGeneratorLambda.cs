using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.DependencyInjection;
using Toxon.Photography.Data.Config;

namespace Toxon.Photography.Generation;

public class SiteGeneratorLambda
{
    internal static readonly TimeSpan ExpirationPeriod = TimeSpan.FromDays(2);

    private readonly IServiceProvider _serviceProvider;

    public SiteGeneratorLambda()
    {
        var region = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION") ?? RegionEndpoint.EUWest2.SystemName);

        var services = new ServiceCollection();
        ConfigureServices(services, region);
        _serviceProvider = services.BuildServiceProvider();
    }

    private static void ConfigureServices(IServiceCollection services, RegionEndpoint region)
    {
        var s3SigningCredentials = LoadS3SigningCredentials().Result;
        
        services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(region));
        
        services.AddSingleton<IAmazonS3>(new AmazonS3Client(region));
        services.AddKeyedSingleton<IAmazonS3>(DynamoDbImageProvider.S3ClientInjectionKey, new AmazonS3Client(s3SigningCredentials, region));

        services.AddScoped<SiteGenerator>();
        services.AddScoped<DynamoDbImageProvider>();
        services.AddScoped<S3SiteStorer>();
    }

    private static async Task<AWSCredentials> LoadS3SigningCredentials()
    {
        // https://repost.aws/knowledge-center/presigned-url-s3-bucket-expiration
        // The credentials that you can use to create a pre-signed URL include:
        // - AWS Identity and Access Management (IAM) instance profile: Valid up to six hours.
        // - AWS Security Token Service (STS): Valid up to 36 hours when signed by an AWS Identity and Access Management (IAM) user, or valid up to one hour when signed by the root user.
        // - IAM user: Valid up to seven days when using AWS Signature Version 4.
        
        var ssm = new AmazonSimpleSystemsManagementClient();
        var parameters = await ssm.GetParametersAsync(new GetParametersRequest { Names = [ParameterNames.SigningAccessKeyPath, ParameterNames.SigningSecretKeyPath], WithDecryption = true });

        var accessKeyParameter = parameters.Parameters.Single(x => x.Name == ParameterNames.SigningAccessKeyPath);
        var secretKeyParameter = parameters.Parameters.Single(x => x.Name == ParameterNames.SigningSecretKeyPath);

        return new BasicAWSCredentials(accessKeyParameter.Value, secretKeyParameter.Value);
    }

    public async Task FunctionHandlerAsync()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        
        var siteGenerator = scope.ServiceProvider.GetRequiredService<SiteGenerator>();
        var siteStorer = scope.ServiceProvider.GetRequiredService<S3SiteStorer>();
        
        var site = await siteGenerator.GenerateAsync();
        await siteStorer.StoreAsync(site);
    }
}