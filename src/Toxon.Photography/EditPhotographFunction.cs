using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Toxon.Photography.Config;
using Toxon.Photography.Http;
using Toxon.Photography.Models;

namespace Toxon.Photography
{
    public class EditPhotographFunction
    {
        private readonly IAmazonDynamoDB _dynamoDb;

        public EditPhotographFunction()
            : this(new AmazonDynamoDBClient())
        {
        }
        public EditPhotographFunction(IAmazonDynamoDB dynamoDb)
        {
            _dynamoDb = dynamoDb;
        }

        public async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest request)
        {
            if (!ValidateRequest(request, out var errorResponse))
            {
                return errorResponse;
            }

            var idStr = request.PathParameters["id"];
            if (!Guid.TryParse(idStr, out var id))
            {
                return Response.CreateError(HttpStatusCode.BadRequest, "Invalid photograph id");
            }

            var model = BuildModelFromRequest(request.Body);
            var updateDocument = BuildDocumentFromModel(model);

            var photographTable = Table.LoadTable(_dynamoDb, TableNames.Photograph);
            var document = await photographTable.UpdateItemAsync(updateDocument, id, new UpdateItemOperationConfig { ReturnValues = ReturnValues.AllNewAttributes });

            var photograph = ListPhotographsFunction.BuildPhotographFromDocument(document);
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

        internal static PhotographyEditModel BuildModelFromRequest(string body)
        {
            return JsonConvert.DeserializeObject<PhotographyEditModel>(body);
        }

        internal static Document BuildDocumentFromModel(PhotographyEditModel model)
        {
            return new Document
            {
                ["title"] = model.Title,
                ["captureTime"] = model.CaptureTime,
            };
        }

        internal static APIGatewayProxyResponse BuildResponseFromModel(Photograph photograph)
        {
            return Response.CreateJson(HttpStatusCode.Created, photograph);
        }
    }
}
