﻿namespace FakeSurveyGenerator.Application.Shared.Caching;

public interface ICache<T>
{
    Task<T?> GetAsync(string key, CancellationToken cancellationToken);
    Task RemoveAsync(string key, CancellationToken cancellationToken);
    Task SetAsync(string key, T item, int minutesToCache, CancellationToken cancellationToken);
    Task<(bool, T? value)> TryGetValueAsync(string key, CancellationToken cancellationToken);
}