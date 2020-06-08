namespace FakeSurveyGenerator.Application.Common.Caching
{
    public interface IDistributedCacheFactory
    {
        IDistributedCache<T> GetCache<T>();
    }
}
