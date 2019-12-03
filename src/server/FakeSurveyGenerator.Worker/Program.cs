using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Infrastructure;
using FakeSurveyGenerator.Infrastructure.Repositories;
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
                        b => b.MigrationsAssembly(typeof(SurveyContext).Namespace)));

                    services.AddScoped<ISurveyRepository, SurveyRepository>();

                    services.AddHostedService<Worker>();
                });
    }
}
