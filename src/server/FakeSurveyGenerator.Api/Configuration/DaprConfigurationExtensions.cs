using Dapr.Client;
using Dapr.Extensions.Configuration;

namespace FakeSurveyGenerator.Api.Configuration;

internal static class DaprConfigurationExtensions
{
    private const string DaprHttpPortEnvVar = "DAPR_HTTP_PORT";

    public static IHostApplicationBuilder AddDaprConfiguration(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDaprClient();

        // Only configure the Dapr secret store when a Dapr sidecar is present.
        // The sidecar injects DAPR_HTTP_PORT into the process environment when attached.
        if (Environment.GetEnvironmentVariable(DaprHttpPortEnvVar) is not null)
            builder.Configuration.AddDaprSecretStore("secrets", new DaprClientBuilder().Build(), TimeSpan.FromSeconds(10));

        return builder;
    }
}