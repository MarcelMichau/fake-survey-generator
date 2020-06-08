using FakeSurveyGenerator.Application;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.API.Builders
{
    internal static class ValidationBuilder
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
