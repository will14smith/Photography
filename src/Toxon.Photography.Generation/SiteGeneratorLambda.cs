using Amazon;
using Amazon.DynamoDBv2;
using Amazon.S3;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.DependencyInjection;
using Toxon.Photography.Data.Config;
using Toxon.Photography.Generation.Stories;

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
        var cloudFrontPrivateKey = LoadCloudFrontPrivateKeyAsync().Result;
        
        services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(region));
        services.AddSingleton<IAmazonS3>(new AmazonS3Client(region));
        services.AddSingleton(_ => new CloudFrontSignedUrlGenerator(
            ParameterNames.CloudFrontImageBaseUrl,
            ParameterNames.CloudFrontImageKeyPairId,
            cloudFrontPrivateKey,
            ExpirationPeriod));

        services.AddScoped<SiteGenerator>();
        services.AddScoped<DynamoDbImageProvider>();
        services.AddScoped<StoryProvider>();
        services.AddScoped<S3SiteStorer>();
    }

    private static async Task<string> LoadCloudFrontPrivateKeyAsync()
    {
        var ssm = new AmazonSimpleSystemsManagementClient();
        var response = await ssm.GetParameterAsync(new GetParameterRequest
        {
            Name = ParameterNames.CloudFrontPrivateKeyPath,
            WithDecryption = true
        });

        return response.Parameter.Value;
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