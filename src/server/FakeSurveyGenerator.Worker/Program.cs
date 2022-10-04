using FakeSurveyGenerator.Application;
using FakeSurveyGenerator.Infrastructure;

namespace FakeSurveyGenerator.Worker;

internal static class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddInfrastructure(hostContext.Configuration);
                services.AddApplication();

                services.AddHostedService<Worker>();
            });
}