﻿namespace FakeSurveyGenerator.Application.Common.Caching;

public interface ICache<T>
{
    Task<T> GetAsync(string key, CancellationToken cancellationToken);
    Task RemoveAsync(string key, CancellationToken cancellationToken);
    Task SetAsync(string key, T item, int minutesToCache, CancellationToken cancellationToken);
    Task<(bool Found, T Value)> TryGetValueAsync(string key, CancellationToken cancellationToken);
}