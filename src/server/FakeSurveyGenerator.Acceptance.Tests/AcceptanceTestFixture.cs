using Aspire.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TUnit.Core.Interfaces;

namespace FakeSurveyGenerator.Acceptance.Tests;

public sealed class AcceptanceTestFixture : IAsyncInitializer, IAsyncDisposable
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(300);
    public DistributedApplication? App;

    public async Task InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.FakeSurveyGenerator_AppHost>();

        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            // Override the logging filters from the app's configuration
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
        });

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        App = await appHost.BuildAsync();

        await App.StartAsync().WaitAsync(DefaultTimeout);

        await App.ResourceNotifications.WaitForResourceHealthyAsync("ui").WaitAsync(DefaultTimeout);
    }

    public async ValueTask DisposeAsync()
    {
        await App!.StopAsync();
    }
}