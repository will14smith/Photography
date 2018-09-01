using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.SimpleNotificationService;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Toxon.Photography.Config;
using Toxon.Photography.Data;
using Toxon.Photography.Http;
using Toxon.Photography.Models;

namespace Toxon.Photography
{
    public class CreatePhotographFunction
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly IAmazonSimpleNotificationService _sns;

        public CreatePhotographFunction()
            : this(new AmazonDynamoDBClient(), new AmazonSimpleNotificationServiceClient())
        {
        }
        public CreatePhotographFunction(IAmazonDynamoDB dynamoDb, IAmazonSimpleNotificationService sns)
        {
            _dynamoDb = dynamoDb;
            _sns = sns;
        }

        public async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest request)
        {
            if (!ValidateRequest(request, out var errorResponse))
            {
                return errorResponse;
            }

            var model = BuildModelFromRequest(request.Body);
            var photograph = BuildPhotographyFromModel(model);

            var photographTable = Table.LoadTable(_dynamoDb, TableNames.Photograph);
            await photographTable.PutItemAsync(PhotographSerialization.ToDocument(photograph));

            await _sns.PublishAsync(TopicArns.ImageProcessor, JsonConvert.SerializeObject(new ImageProcessorMessage { PhotographId = photograph.Id, Image = photograph.Images.Single() }));

            return BuildResponseFromModel(photograph);
        }

        internal static bool ValidateRequest(APIGatewayProxyRequest request, out APIGatewayProxyResponse response)
        {
            var headers = request.GetHeaders();
            if (!headers.ContentType.IsSubsetOf(MediaTypeHeaderValue.Parse("application/json")))
            {
                response = Response.CreateError(HttpStatusCode.BadRequest, "Content-Type should be application/json");
                return false;
            }

            response = null;
            return true;
        }

        internal static PhotographyCreateModel BuildModelFromRequest(string body)
        {
            return JsonConvert.DeserializeObject<PhotographyCreateModel>(body);
        }

        internal static Photograph BuildPhotographyFromModel(PhotographyCreateModel model)
        {
            return new Photograph
            {
                Title = model.Title,
                Images = new[] { new Image { Type = ImageType.Full, ObjectKey = model.ImageKey } },

                CaptureTime = model.CaptureTime,
                UploadTime = DateTime.UtcNow,
            };
        }

        internal static APIGatewayProxyResponse BuildResponseFromModel(Photograph photograph)
        {
            return Response.CreateJson(HttpStatusCode.Created, photograph);
        }
    }
}
