namespace FakeSurveyGenerator.Infrastructure.Caching;
internal sealed class CacheOptions
{
    public const string Cache = "Cache";

    public string RedisUrl { get; set; }
    public string RedisPassword { get; set; }
    public bool RedisSsl { get; set; }
    public int RedisDefaultDatabase { get; set; }
}
