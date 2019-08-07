using System;
using System.Linq;
using FakeSurveyGenerator.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.API.Tests.Integration
{
    public class InMemoryDatabaseWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        //Just a different implementation of the below method

        //protected override IHost CreateHost(IHostBuilder builder)
        //{
        //    Environment.SetEnvironmentVariable("ConnectionStrings__SurveyContext", "Server=sqlserver;Database=FakeSurveyGenerator;user id=SA;pwd=<YourStrong!Passw0rd>;ConnectRetryCount=0");

        //    builder.ConfigureServices(services =>
        //    {
        //        // Workaround because of https://github.com/aspnet/AspNetCore/issues/12360
        //        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SurveyContext>));
        //        if (descriptor != null)
        //        {
        //            services.Remove(descriptor);
        //        }

        //        services.AddEntityFrameworkInMemoryDatabase();

        //        services.AddDbContext<SurveyContext>(options =>
        //        {
        //            options.UseInMemoryDatabase("InMemoryDbForTesting");
        //            options.UseInternalServiceProvider(services.BuildServiceProvider());
        //        });
        //    });

        //    return base.CreateHost(builder);
        //}


        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__SurveyContext", "Server=sqlserver;Database=FakeSurveyGenerator;user id=SA;pwd=<YourStrong!Passw0rd>;ConnectRetryCount=0");
            Environment.SetEnvironmentVariable("REDIS_URL", "Test");
            Environment.SetEnvironmentVariable("REDIS_PASSWORD", "Test");
            Environment.SetEnvironmentVariable("REDIS_SSL", "false");

            builder.ConfigureServices(services =>
            {

                // Workaround because of https://github.com/aspnet/AspNetCore/issues/12360:
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SurveyContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddEntityFrameworkInMemoryDatabase();

                services.AddDbContext<SurveyContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                    options.UseInternalServiceProvider(services.BuildServiceProvider());
                });
            });
        }
    }
}
