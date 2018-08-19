using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Toxon.Photography.Http;
using Toxon.Photography.Models;

namespace Toxon.Photography
{
    public class CreatePhotographFunction
    {
        private readonly IAmazonDynamoDB _dynamoDb;

        public CreatePhotographFunction()
            : this(new AmazonDynamoDBClient())
        {
        }
        public CreatePhotographFunction(IAmazonDynamoDB dynamoDb)
        {
            _dynamoDb = dynamoDb;
        }

        public async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest request)
        {
            if (!ValidateRequest(request, out var errorResponse))
            {
                return errorResponse;
            }

            var photograph = BuildModelFromRequest(request.Body);

            var photographTable = Table.LoadTable(_dynamoDb, TableNames.Photograph);
            await photographTable.PutItemAsync(BuildDocumentFromModel(photograph));

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

        internal static Photograph BuildModelFromRequest(string body)
        {
            var model = JsonConvert.DeserializeObject<Photograph>(body);

            model.Id = Guid.NewGuid();

            return model;
        }

        internal static Document BuildDocumentFromModel(Photograph photograph)
        {
            return new Document
            {
                ["id"] = photograph.Id.ToString(),
                ["title"] = photograph.Title,
            };
        }

        internal static APIGatewayProxyResponse BuildResponseFromModel(Photograph photograph)
        {
            return Response.CreateJson(HttpStatusCode.Created, new
            {
                photograph.Id,
            });
        }
    }
}
