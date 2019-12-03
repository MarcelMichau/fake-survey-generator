using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.API.Builders
{
    public static class MediatRBuilder
    {
        public static IServiceCollection AddMediatRConfiguration(this IServiceCollection services)
        {
            var currentAssembly = typeof(Startup).GetTypeInfo().Assembly;

            services.AddMediatR(currentAssembly);

            return services;
        }
    }
}
