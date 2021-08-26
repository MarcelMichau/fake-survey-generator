using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services;
using Microsoft.Extensions.Configuration;
using TechTalk.SpecFlow;

namespace FakeSurveyGenerator.Acceptance.Tests.Hooks
{
    [Binding]
    public class DockerComposeHooks
    {
        private static ICompositeService _compositeService;

        [BeforeTestRun]
        public static void DockerComposeUp()
        {
            var config = LoadConfiguration();

            var dockerComposeFileName = config.GetValue<string>("DockerComposeFileName");
            var dockerComposeOverrideFileName = DetermineOverrideFileName(config);
            var dockerComposePath = GetDockerComposeFileLocation(dockerComposeFileName);
            var dockerComposeOverridePath = GetDockerComposeFileLocation(dockerComposeOverrideFileName);

            var applicationUrl = config.GetValue<string>("FakeSurveyGeneratorUI:BaseAddress");

            _compositeService = new Builder()
                .UseContainer()
                .UseCompose()
                .FromFile(dockerComposePath)
                .FromFile(dockerComposeOverridePath)
                .RemoveOrphans()
                .WaitForHttp("api", $"{applicationUrl}/health/ready",
                    continuation: (response, _) => response.Code != HttpStatusCode.OK ? 2000 : 0)
                .WaitForHttp("ui", $"{applicationUrl}",
                    continuation: (response, _) => response.Code != HttpStatusCode.OK ? 2000 : 0)
                .Build()
                .Start();
        }

        [AfterTestRun]
        public static void DockerComposeDown()
        {
            _compositeService.Stop();
            _compositeService.Dispose();
        }

        private static IConfiguration LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
        }

        private static string DetermineOverrideFileName(IConfiguration config)
        {
            return config.GetValue<string>(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? "DockerComposeOverrideLinuxFileName"
                : "DockerComposeOverrideWindowsFileName");
        }

        private static string GetDockerComposeFileLocation(string dockerComposeFileName)
        {
            var directory = Directory.GetCurrentDirectory();

            while (!Directory.EnumerateFiles(directory, "*.yml").Any(s => s.EndsWith(dockerComposeFileName)))
            {
                directory = directory.Substring(0, directory.LastIndexOf(Path.DirectorySeparatorChar));
            }

            return Path.Combine(directory, dockerComposeFileName);
        }
    }
}