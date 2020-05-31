using System;
using FakeSurveyGenerator.Application;
using FakeSurveyGenerator.Infrastructure;
using FakeSurveyGenerator.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FakeSurveyGenerator.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var connectionString = hostContext.Configuration.GetConnectionString(nameof(SurveyContext));

                    services.AddDbContext<SurveyContext>
                    (options => options.UseSqlServer(connectionString,
                        sqlServerOptions =>
                        {
                            sqlServerOptions.MigrationsAssembly(typeof(SurveyContext).Namespace);
                            sqlServerOptions.EnableRetryOnFailure(15, TimeSpan.FromSeconds(30),
                                null);
                        }));

                    services.AddInfrastructure(hostContext.Configuration);
                    services.AddApplication();

                    services.AddHostedService<Worker>();
                });
    }
}
