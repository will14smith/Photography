using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Toxon.Photography.Config;
using Toxon.Photography.Data;
using Toxon.Photography.Http;

namespace Toxon.Photography
{
    public class EditLayoutFunction
    {
        private readonly IAmazonDynamoDB _dynamoDb;

        public EditLayoutFunction()
            : this(new AmazonDynamoDBClient())
        {
        }

        public EditLayoutFunction(IAmazonDynamoDB dynamoDb)
        {
            _dynamoDb = dynamoDb;
        }

        public async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest request)
        {
            if (!ValidateRequest(request, out var errorResponse))
            {
                return errorResponse;
            }

            var model = BuildModelFromRequest(request.Body);

            var photographTable = Table.LoadTable(_dynamoDb, TableNames.Photograph);
            var allIds = await GetAllIds(photographTable);

            foreach (var id in allIds)
            {
                var layout = model.TryGetValue(id, out var l) ? l : (int?)null;

                await SetLayoutForPhotograph(photographTable, id, layout);
            }

            return Response.CreateJson(HttpStatusCode.Accepted, null);
        }

        private static bool ValidateRequest(APIGatewayProxyRequest request, out APIGatewayProxyResponse response)
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

        private static IReadOnlyDictionary<Guid, int> BuildModelFromRequest(string body)
        {
            return JsonConvert.DeserializeObject<Dictionary<Guid, int>>(body);
        }

        private async Task<IReadOnlyCollection<Guid>> GetAllIds(Table table)
        {
            var search = table.Scan(new ScanOperationConfig
            {
                AttributesToGet = new List<string> { PhotographSerialization.Fields.Id },
                Select = SelectValues.SpecificAttributes,
            });

            var documents = await search.GetAllAsync();

            return documents.Select(x => x[PhotographSerialization.Fields.Id].AsGuid()).ToList();
        }

        private static async Task SetLayoutForPhotograph(Table table, Guid id, int? layout)
        {
            var document = new Document
            {
                [PhotographSerialization.Fields.LayoutPosition] = layout,
            };

            await table.UpdateItemAsync(document, id, new UpdateItemOperationConfig { ReturnValues = ReturnValues.None });
        }
    }
}
