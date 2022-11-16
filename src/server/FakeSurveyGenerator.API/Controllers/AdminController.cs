using Dapr.Client;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FakeSurveyGenerator.API.Controllers;

[SwaggerTag("Administrative operations for testing/diagnostic purposes")]
public sealed class AdminController : ApiController
{
    private readonly IConfiguration _configuration;
    private readonly DaprClient _client;

    public AdminController(IConfiguration configuration, DaprClient client)
    {
        _configuration = configuration;
        _client = client;
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

    [SwaggerOperation("Retrieves a test secret from Azure Key Vault. Used for debugging connection to Azure Key Vault")]
    [HttpGet("keyvaulttest")]
    public async Task<IActionResult> KeyVaultTest()
    {
        var secrets = await _client.GetSecretAsync("azure-key-vault", "HealthCheckSecret");

        var secretValue = string.Join(", ", secrets);

        return Ok(new
        {
            secretValue
        });
    }
}