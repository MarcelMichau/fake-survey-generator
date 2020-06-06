using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace FakeSurveyGenerator.API
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel(options => options.AddServerHeader = false);
                });
    }
}
