using Dapr.Client;
using Dapr.Extensions.Configuration;

namespace FakeSurveyGenerator.Api.Configuration;

internal static class DaprConfigurationExtensions
{
    public static IHostApplicationBuilder AddDaprConfiguration(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDaprClient();

        if (!builder.Configuration.GetValue<bool>("SKIP_DAPR"))
            builder.Configuration.AddDaprSecretStore("secrets", new DaprClientBuilder().Build(), TimeSpan.FromSeconds(10));

        return builder;
    }
}