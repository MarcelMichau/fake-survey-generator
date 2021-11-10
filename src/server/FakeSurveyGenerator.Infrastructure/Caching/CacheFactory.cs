using System;
using FakeSurveyGenerator.Application.Common.Caching;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.Infrastructure.Caching;

internal sealed class CacheFactory : ICacheFactory
{
    private readonly IServiceProvider _serviceProvider;

    public CacheFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ICache<T> GetCache<T>() => _serviceProvider.GetRequiredService<ICache<T>>();
}