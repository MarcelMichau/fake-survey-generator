using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.API.Builders
{
    public static class AutoMapperBuilder
    {
        public static IServiceCollection AddAutoMapperConfiguration(this IServiceCollection services)
        {
            var currentAssembly = typeof(Startup).GetTypeInfo().Assembly;

            services.AddAutoMapper(currentAssembly);

            return services;
        }
    }
}
