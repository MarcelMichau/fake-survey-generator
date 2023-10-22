namespace FakeSurveyGenerator.Application.Shared.Caching;

public interface ICacheFactory
{
    ICache<T> GetCache<T>();
}