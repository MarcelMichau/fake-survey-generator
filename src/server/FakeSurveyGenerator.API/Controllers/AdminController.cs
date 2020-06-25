using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FakeSurveyGenerator.API.Controllers
{
    public sealed class AdminController : ApiController
    {
        private readonly IConfiguration _configuration;

        public AdminController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Returns API version information
        /// </summary>
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
                ThisAssembly.RootNamespace
            });
        }

        /// <summary>
        /// Returns a 200 OK Result. Used for testing network latency and as a sanity check
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok();
        }

        /// <summary>
        /// Retrieves a test secret from Azure Key Vault. Used for debugging connection to Azure Key Vault
        /// </summary>
        [HttpGet("keyvaulttest")]
        public IActionResult KeyVaultTest()
        {
            var secretValue = _configuration.GetValue<string>("AZURE_KEY_VAULT_TEST_SECRET");

            return Ok(new
            {
                secretValue
            });
        }
    }
}