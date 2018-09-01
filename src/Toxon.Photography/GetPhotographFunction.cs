using System;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Toxon.Photography.Config;
using Toxon.Photography.Data;
using Toxon.Photography.Http;

namespace Toxon.Photography
{
    public class GetPhotographFunction
    {
        private readonly IAmazonDynamoDB _dynamoDb;

        public GetPhotographFunction()
            : this(new AmazonDynamoDBClient())
        {
        }
        public GetPhotographFunction(IAmazonDynamoDB dynamoDb)
        {
            _dynamoDb = dynamoDb;
        }

        public async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest request)
        {
            var idStr = request.PathParameters["id"];
            if (!Guid.TryParse(idStr, out var id))
            {
                return Response.CreateError(HttpStatusCode.BadRequest, "Invalid photograph id");
            }

            var photographTable = Table.LoadTable(_dynamoDb, TableNames.Photograph);
            var document = await photographTable.GetItemAsync(id);
            if (document == null)
            {
                return Response.CreateError(HttpStatusCode.NotFound, "No photograph with the requested id was found");
            }

            var model = PhotographSerialization.FromDocument(document);

            return BuildResponseFromModel(model);
        }
        
        internal static APIGatewayProxyResponse BuildResponseFromModel(Photograph model)
        {
            return Response.CreateJson(HttpStatusCode.OK, model);
        }
    }
}
