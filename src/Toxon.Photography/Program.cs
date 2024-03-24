using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.EventBridge;
using Amazon.Lambda;

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
builder.Services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(region));
builder.Services.AddSingleton<IAmazonEventBridge>(new AmazonEventBridgeClient(region));
builder.Services.AddSingleton<IAmazonLambda>(new AmazonLambdaClient(region));
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();