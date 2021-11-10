namespace FakeSurveyGenerator.Application.Common.Caching;

public interface ICacheFactory
{
    ICache<T> GetCache<T>();
}