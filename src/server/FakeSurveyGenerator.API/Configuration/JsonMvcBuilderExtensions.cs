using System.Text.Json;

namespace FakeSurveyGenerator.API.Configuration;

internal static class JsonMvcBuilderExtensions
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