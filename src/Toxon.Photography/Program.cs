using System.Text.Json.Serialization;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.DynamoDBv2;
using Amazon.EventBridge;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Lambda;
using Amazon.S3;
using SixLabors.ImageSharp.Metadata;
using Toxon.Photography.ImageProcessing;

var builder = WebApplication.CreateBuilder(args);

builder.Logging
        .ClearProviders()
        .AddJsonConsole();

var region = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION") ?? RegionEndpoint.EUWest2.SystemName);

builder.Services
        .AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()))
        .AddControllers()
        .AddJsonOptions(options =>
        {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

builder.Services.AddDefaultAWSOptions(new AWSOptions { Region = region });

builder.Services.AddAWSService<IAmazonBedrockRuntime>();
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddAWSService<IAmazonEventBridge>();
builder.Services.AddAWSService<IAmazonLambda>();
builder.Services.AddAWSService<IAmazonS3>();

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

builder.Services.AddTransient<MetadataProcessor>();
builder.Services.AddTransient<ThumbnailProcessor>();
builder.Services.AddTransient<TitleSuggestionProcessor>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();