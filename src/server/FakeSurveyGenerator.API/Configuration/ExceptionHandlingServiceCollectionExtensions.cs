using FakeSurveyGenerator.API.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.API.Configuration
{
    internal static class ExceptionHandlingServiceCollectionExtensions
    {
        public static IMvcBuilder AddExceptionHandlingConfiguration(this IMvcBuilder builder)
        {
            builder.AddMvcOptions(options => options.Filters.Add(new ApiExceptionFilterAttribute()));

            return builder;
        }
    }
}
