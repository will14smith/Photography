using Amazon.Lambda;
using Amazon.Lambda.Model;
using Microsoft.AspNetCore.Mvc;
using Toxon.Photography.Data.Config;

namespace Toxon.Photography.Controllers;

[ApiController]
[Route("site-generator")]
public class SiteGeneratorController(IAmazonLambda lambda) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Generate()
    {
        await lambda.InvokeAsync(new InvokeRequest
        {
            FunctionName = LambdaNames.SiteGenerator,
            InvocationType = InvocationType.RequestResponse,
            Payload = "{}"
        });
            
        return NoContent();
    }
}