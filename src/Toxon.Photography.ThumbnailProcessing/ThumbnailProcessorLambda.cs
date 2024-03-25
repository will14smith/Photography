using Amazon;
using Amazon.DynamoDBv2;
using Amazon.EventBridge;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;

[assembly: Amazon.Lambda.Core.LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Toxon.Photography.ThumbnailProcessing;

public class ThumbnailProcessorLambda
{
    private readonly IServiceProvider _serviceProvider;

    public ThumbnailProcessorLambda()
    {
        var region = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION") ?? RegionEndpoint.EUWest2.SystemName);

        var services = new ServiceCollection();
        ConfigureServices(services, region);
        _serviceProvider = services.BuildServiceProvider();
    }

    private static void ConfigureServices(IServiceCollection services, RegionEndpoint region)
    {
        services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(region));
        services.AddSingleton<IAmazonEventBridge>(new AmazonEventBridgeClient(region));
        services.AddSingleton<IAmazonS3>(new AmazonS3Client(region));

        services.AddScoped<ThumbnailProcessor>();
    }

    public async Task FunctionHandlerAsync(FunctionInput input)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var processor = scope.ServiceProvider.GetRequiredService<ThumbnailProcessor>();
        await processor.Process(input.Detail.Photograph.Id);
    }
}