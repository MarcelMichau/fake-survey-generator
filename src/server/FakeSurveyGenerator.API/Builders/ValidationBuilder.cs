using FakeSurveyGenerator.Application.Common.Interfaces;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.API.Builders
{
    public static class ValidationBuilder
    {
        public static IMvcBuilder AddValidationConfiguration(this IMvcBuilder builder)
        {
            builder.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<IConnectionString>());

            return builder;
        }
    }
}
