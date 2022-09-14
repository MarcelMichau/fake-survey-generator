using FakeSurveyGenerator.API.Filters;

namespace FakeSurveyGenerator.API.Configuration;

internal static class ExceptionHandlingServiceCollectionExtensions
{
    public static IMvcBuilder AddExceptionHandlingConfiguration(this IMvcBuilder builder)
    {
        builder.AddMvcOptions(options => options.Filters.Add(new ApiExceptionFilterAttribute()));

        return builder;
    }
}