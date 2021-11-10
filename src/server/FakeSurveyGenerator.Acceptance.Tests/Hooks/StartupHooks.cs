using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services;
using Microsoft.Extensions.Configuration;
using TechTalk.SpecFlow;

namespace FakeSurveyGenerator.Acceptance.Tests.Hooks;

[Binding]
public class StartupHooks
{
    private static ICompositeService _compositeService;
    private static readonly IConfiguration Configuration;
    private static readonly bool IsRunningInDocker;

    static StartupHooks()
    {
        Configuration = LoadConfiguration();
        IsRunningInDocker = Configuration.GetValue<string>("FakeSurveyGeneratorUI:BaseAddress").Contains("localhost");
    }

    [BeforeTestRun]
    public static void StartTestRun()
    {
        if (!IsRunningInDocker)
            return;

        var dockerComposeFileName = Configuration.GetValue<string>("DockerComposeFileName");
        var dockerComposeOverrideFileName = DetermineOverrideFileName(Configuration);
        var dockerComposePath = GetDockerComposeFileLocation(dockerComposeFileName);
        var dockerComposeOverridePath = GetDockerComposeFileLocation(dockerComposeOverrideFileName);

        var applicationUrl = Configuration.GetValue<string>("FakeSurveyGeneratorUI:BaseAddress");

        _compositeService = new Builder()
            .UseContainer()
            .UseCompose()
            .FromFile(dockerComposePath)
            .FromFile(dockerComposeOverridePath)
            .RemoveOrphans()
            .WaitForHttp("fake-survey-generator-api", $"{applicationUrl}/health/ready",
                continuation: (response, _) => response.Code != HttpStatusCode.OK ? 2000 : 0)
            .WaitForHttp("fake-survey-generator-ui", $"{applicationUrl}",
                continuation: (response, _) => response.Code != HttpStatusCode.OK ? 2000 : 0)
            .Build()
            .Start();
    }

    [AfterTestRun]
    public static void StopTestRun()
    {
        if (!IsRunningInDocker)
            return;

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