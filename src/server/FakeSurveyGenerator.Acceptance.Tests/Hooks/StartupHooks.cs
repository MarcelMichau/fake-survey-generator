using System.Configuration;
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
    private static ICompositeService? _compositeService;
    private static readonly IConfiguration Configuration;
    private static readonly bool IsRunningInDocker;

    static StartupHooks()
    {
        Configuration = LoadConfiguration();
        IsRunningInDocker =
            (Configuration.GetValue<string>("FakeSurveyGeneratorUI:BaseAddress") ??
             throw new InvalidOperationException("BaseAddress for FakeSurveyGeneratorUI was not found in config"))
            .Contains("localhost");
    }

    [BeforeTestRun]
    public static void StartTestRun()
    {
        if (!IsRunningInDocker)
            return;

        var dockerComposeFileName = Configuration.GetValue<string>("DockerComposeFileName") ??
                                    throw new ConfigurationErrorsException(
                                        "DockerComposeFileName was not found in config");
        var dockerComposeOverrideFileName = DetermineOverrideFileName(Configuration);
        var dockerComposePath = GetDockerComposeFileLocation(dockerComposeFileName);
        var dockerComposeOverridePath = GetDockerComposeFileLocation(dockerComposeOverrideFileName);

        var uiContainerUrl = Configuration.GetValue<string>("FakeSurveyGeneratorUI:BaseAddress");
        var apiContainerUrl = Configuration.GetValue<string>("FakeSurveyGeneratorAPI:BaseAddress");

        _compositeService = new Builder()
            .UseContainer()
            .UseCompose()
            .FromFile(dockerComposePath)
            .FromFile(dockerComposeOverridePath)
            .RemoveOrphans()
            .WaitForHttp("fake-survey-generator-api", $"{apiContainerUrl}/health/ready",
                continuation: (response, _) => response.Code == HttpStatusCode.OK ? 0 : 2000)
            .WaitForHttp("fake-survey-generator-ui", $"{uiContainerUrl}",
                continuation: (response, _) => response.Code == HttpStatusCode.OK ? 0 : 2000)
            .Build()
            .Start();
    }

    [AfterTestRun]
    public static void StopTestRun()
    {
        if (!IsRunningInDocker)
            return;

        if (_compositeService == null) return;

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
            : "DockerComposeOverrideWindowsFileName") ?? throw new InvalidOperationException(
            "DockerComposeOverrideLinuxFileName or DockerComposeOverrideWindowsFileName were not found in config");
    }

    private static string GetDockerComposeFileLocation(string dockerComposeFileName)
    {
        var directory = Directory.GetCurrentDirectory();

        while (!Directory.EnumerateFiles(directory, "*.yml").Any(s => s.EndsWith(dockerComposeFileName)))
        {
            directory = directory[..directory.LastIndexOf(Path.DirectorySeparatorChar)];
        }

        return Path.Combine(directory, dockerComposeFileName);
    }
}