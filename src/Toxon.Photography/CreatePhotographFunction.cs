using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.S3;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Toxon.Photography.Http;
using Toxon.Photography.Models;

namespace Toxon.Photography
{
    public class CreatePhotographFunction
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly IAmazonS3 _s3;

        public CreatePhotographFunction()
            : this(new AmazonDynamoDBClient(), new AmazonS3Client())
        {
        }
        public CreatePhotographFunction(IAmazonDynamoDB dynamoDb, IAmazonS3 s3)
        {
            _dynamoDb = dynamoDb;
            _s3 = s3;
        }

        public async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest request)
        {
            if (!ValidateRequest(request, out var errorResponse))
            {
                return errorResponse;
            }

            var model = BuildModelFromRequest(request.Body);

            var image = await SaveImageAsync(model);
            var photograph = BuildPhotographyFromModel(model, image);

            var photographTable = Table.LoadTable(_dynamoDb, TableNames.Photograph);
            await photographTable.PutItemAsync(BuildDocumentFromPhotograph(photograph));

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

        internal static PhotographyInputModel BuildModelFromRequest(string body)
        {
            return JsonConvert.DeserializeObject<PhotographyInputModel>(body);
        }

        private async Task<Image> SaveImageAsync(PhotographyInputModel model)
        {
            var bucket = BucketNames.Images;
            var key = Guid.NewGuid().ToString();

            using (var stream = new MemoryStream(model.Image))
            {
                await _s3.UploadObjectFromStreamAsync(bucket, key, stream, new Dictionary<string, object> { { "ContentType", model.ImageContentType } });
            }

            return new Image { Type = ImageType.Full, ObjectKey = key };
        }

        internal static Photograph BuildPhotographyFromModel(PhotographyInputModel model, Image image)
        {
            return new Photograph
            {
                Title = model.Title,
                Images = new[] { image }
            };
        }

        internal static Document BuildDocumentFromPhotograph(Photograph photograph)
        {
            return new Document
            {
                ["id"] = photograph.Id.ToString(),
                ["title"] = photograph.Title,
                ["images"] = new DynamoDBList(photograph.Images.Select(BuildDocumentFromImage)),
            };
        }

        internal static Document BuildDocumentFromImage(Image image)
        {
            return new Document
            {
                ["type"] = image.Type.ToString(),
                ["objectKey"] = image.ObjectKey,
            };
        }

        internal static APIGatewayProxyResponse BuildResponseFromModel(Photograph photograph)
        {
            return Response.CreateJson(HttpStatusCode.Created, photograph);
        }
    }
}
