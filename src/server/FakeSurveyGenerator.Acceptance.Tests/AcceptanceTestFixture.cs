using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Core.Interfaces;

namespace FakeSurveyGenerator.Acceptance.Tests;
public sealed class AcceptanceTestFixture : IAsyncInitializer, IAsyncDisposable
{
    public DistributedApplication? App;

    public async Task InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.FakeSurveyGenerator_AppHost>();

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        App = await appHost.BuildAsync();
        var resourceNotificationService = App.Services.GetRequiredService<ResourceNotificationService>();
        await App.StartAsync();

        await resourceNotificationService
            .WaitForResourceAsync("fake-survey-generator-ui", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(30));

        await App.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await App!.StopAsync();
    }
}
