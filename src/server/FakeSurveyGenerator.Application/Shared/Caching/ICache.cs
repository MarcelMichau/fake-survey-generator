namespace FakeSurveyGenerator.Application.Shared.Caching;

public interface ICache<T>
{
    ValueTask<T> GetOrCreateAsync(string key, Func<CancellationToken, ValueTask<T>> factory, CancellationToken cancellationToken);
    ValueTask RemoveAsync(string key, CancellationToken cancellationToken);
    ValueTask SetAsync(string key, T item, int minutesToCache, CancellationToken cancellationToken);
}