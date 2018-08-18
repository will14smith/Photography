using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;

namespace Toxon.Photography.Http
{
   public static class APIGatewayProxyRequestExtensions
    {
        public static RequestHeaders GetHeaders(this APIGatewayProxyRequest request)
        {
            var headers = new HeaderDictionary();

            foreach (var header in request.Headers)
            {
                headers[header.Key] = header.Value;
            }

            return new RequestHeaders(headers);
        }
    }
}
