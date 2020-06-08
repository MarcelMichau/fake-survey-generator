using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.API.Builders
{
    internal static class JsonBuilder
    {
        public static IMvcBuilder AddJsonConfiguration(this IMvcBuilder builder)
        {
            builder.AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            return builder;
        }
    }
}
