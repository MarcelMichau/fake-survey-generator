using System.Reflection;
using AutoMapper;
using FakeSurveyGenerator.Application.Common.Behaviours;
using FakeSurveyGenerator.Application.Common.Caching;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FakeSurveyGenerator.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddMediatR(Assembly.GetExecutingAssembly());

            services.TryAddSingleton(typeof(IDistributedCache<>), typeof(DistributedCache<>));
            services.TryAddSingleton<IDistributedCacheFactory, DistributedCacheFactory>();

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}
