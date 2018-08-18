using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

namespace Toxon.Photography.Http
{
    public static class ErrorResponse
    {
        public static APIGatewayProxyResponse Create(HttpStatusCode code, string message)
        {
            var body = JsonConvert.SerializeObject(new
            {
                Error = true,
                Message = message,
            });

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)code,
                Headers = new Dictionary<string, string>
                {
                    ["Content-Type"] = "application/json",
                },

                Body = body,
            };
        }
    }
}
