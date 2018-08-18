using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;

namespace Toxon.Photography
{
    public class HelloFunction
    {
        public APIGatewayProxyResponse Handle(APIGatewayProxyRequest request)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "text/html" } },

                Body = "<strong>Hello World!</strong>",
            };
        }
    }
}
