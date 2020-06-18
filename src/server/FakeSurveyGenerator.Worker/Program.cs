using FakeSurveyGenerator.Application;
using FakeSurveyGenerator.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FakeSurveyGenerator.Worker
{
    internal sealed class Program
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
}
