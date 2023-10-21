using FakeSurveyGenerator.Application.Shared.Caching;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.Application.Infrastructure.Caching;

internal sealed class CacheFactory(IServiceProvider serviceProvider) : ICacheFactory
{
    public ICache<T> GetCache<T>() => serviceProvider.GetRequiredService<ICache<T>>();
}