using FakeSurveyGenerator.Application.Shared.Identity;
using FakeSurveyGenerator.Application.TestHelpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FakeSurveyGenerator.Api.Tests.Integration.Setup;

public sealed class IntegrationTestWebApplicationFactory(TestContainerSettings settings)
    : WebApplicationFactory<Program>
{
    private readonly TestContainerSettings _settings = settings ?? throw new ArgumentNullException(nameof(settings));

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                {
                    "ASPNETCORE_ENVIRONMENT", "Production"
                }, // Run integration tests as close as possible to how code will be run in Production
                { "SKIP_DAPR", "true" }, // Do not configure Dapr components for integration tests

                { "ConnectionStrings:database", _settings.SqlServerConnectionString },
                { "ConnectionStrings:cache", _settings.RedisConnectionString },
                { "IDENTITY_PROVIDER_URL", "https://somenonexistentdomain.com" }
            }!);
        });

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging => { logging.ClearProviders(); });

        builder.ConfigureTestServices(ConfigureMockServices);
    }

    private static void ConfigureMockServices(IServiceCollection services)
    {
        var mockUserService = Substitute.For<IUserService>();
        mockUserService.GetUserInfo(Arg.Any<CancellationToken>()).Returns(new TestUser());
        mockUserService.GetUserIdentity().Returns(new TestUser().Id);

        services.AddScoped(_ => mockUserService);
    }
}