using FakeSurveyGenerator.Application;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.API.Configuration
{
    internal static class ValidationMvcBuilderExtensions
    {
        public static IMvcBuilder AddValidationConfiguration(this IMvcBuilder builder)
        {
            builder.AddFluentValidation(fv =>
            {
                fv.RegisterValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
                fv.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
            });

            return builder;
        }
    }
}
