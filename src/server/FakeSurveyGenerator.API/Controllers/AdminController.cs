using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FakeSurveyGenerator.API.Controllers;

[SwaggerTag("Administrative operations for testing/diagnostic purposes")]
public sealed class AdminController : ApiController
{
    private readonly IConfiguration _configuration;

    public AdminController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [SwaggerOperation("Returns API version information")]
    [HttpGet("version")]
    public IActionResult Version()
    {
        return Ok(new
        {
            ThisAssembly.AssemblyVersion,
            ThisAssembly.AssemblyFileVersion,
            ThisAssembly.AssemblyInformationalVersion,
            ThisAssembly.AssemblyName,
            ThisAssembly.AssemblyTitle,
            ThisAssembly.AssemblyConfiguration,
            ThisAssembly.RootNamespace,
            ThisAssembly.GitCommitDate,
            ThisAssembly.GitCommitId
        });
    }

    [SwaggerOperation("Returns a 200 OK Result. Used for testing network latency and as a sanity check")]
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok();
    }

    [SwaggerOperation("Retrieves a test secret from Dapr Secret Store. Uses local file in Development & Azure Key Vault in Production")]
    [HttpGet("secret-test")]
    public IActionResult SecretTest()
    {
        var secretValue = _configuration.GetValue<string>("HealthCheckSecret");

        return Ok(new
        {
            secretValue
        });
    }
}