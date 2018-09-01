using System.Collections.Generic;
using System.Linq;
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
    public class ListPhotographsFunction
    {
        private readonly IAmazonDynamoDB _dynamoDb;

        public ListPhotographsFunction()
            : this(new AmazonDynamoDBClient())
        {
        }
        public ListPhotographsFunction(IAmazonDynamoDB dynamoDb)
        {
            _dynamoDb = dynamoDb;
        }

        public async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest request)
        {
            var photographTable = Table.LoadTable(_dynamoDb, TableNames.Photograph);
            var search = photographTable.Scan(new ScanFilter());

            var documents = await search.GetAllAsync();
            var models = documents.Select(PhotographSerialization.FromDocument);

            return BuildResponseFromModels(models);
        }

        internal static APIGatewayProxyResponse BuildResponseFromModels(IEnumerable<Photograph> models)
        {
            return Response.CreateJson(HttpStatusCode.OK, models.ToList());
        }
    }
}
