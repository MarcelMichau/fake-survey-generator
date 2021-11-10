using System.Net.NetworkInformation;
using FakeSurveyGenerator.API.Networking;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.API.Configuration;

internal static class ForwardedHeadersServiceCollectionExtensions
{
    public static IServiceCollection AddForwardedHeadersConfiguration(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            foreach (var network in Utilities.GetNetworks(NetworkInterfaceType.Ethernet))
            {
                options.KnownNetworks.Add(network);
            }
        });

        return services;
    }
}