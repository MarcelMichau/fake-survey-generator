using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace FakeSurveyGenerator.Infrastructure.Builders
{
    internal static class CacheBuilder
    {
        public static IServiceCollection AddCacheConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.ConfigurationOptions = new ConfigurationOptions
                {
                    EndPoints = { configuration.GetValue<string>("REDIS_URL") },
                    Password = configuration.GetValue<string>("REDIS_PASSWORD"),
                    Ssl = configuration.GetValue<bool>("REDIS_SSL"),
                    DefaultDatabase = configuration.GetValue<int>("REDIS_DEFAULT_DATABASE")
                };
            });

            return services;
        }
    }
}
