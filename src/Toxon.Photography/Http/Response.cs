using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

namespace Toxon.Photography.Http
{
    public static class Response
    {
        public static APIGatewayProxyResponse CreateJson(HttpStatusCode code, object value)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)code,
                Headers = new Dictionary<string, string>
                {
                    ["Access-Control-Allow-Credentials"] = "true",
                    ["Access-Control-Allow-Origin"] = "*",

                    ["Content-Type"] = "application/json",
                },

                Body = JsonConvert.SerializeObject(value),
            };
        }

        public static APIGatewayProxyResponse CreateError(HttpStatusCode code, string message)
        {
            return CreateJson(code, new
            {
                Error = true,
                Message = message,
            });
        }
    }
}
