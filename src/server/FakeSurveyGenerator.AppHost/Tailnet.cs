namespace FakeSurveyGenerator.AppHost;

/// <summary>
/// Dashboard links for remote access through a Tailscale Service.
/// The ports must match those published by tools/tailnet/tailnet.cs.
/// </summary>
internal static class Tailnet
{
    public const int RedisInsightPort = 5540;

    // Dashboard, UI, API (HTTPS) and Redis Insight.
    private static readonly HashSet<int> PublishedPorts = [17081, 3000, 7223, RedisInsightPort];

    /// <summary>
    /// Adds a tailnet copy of each HTTPS localhost URL whose port the Tailscale Service publishes.
    /// Does nothing when <paramref name="tailnetHost"/> is not set.
    /// </summary>
    public static IResourceBuilder<T> WithTailnetUrls<T>(this IResourceBuilder<T> builder, string? tailnetHost)
        where T : IResourceWithEndpoints
    {
        if (string.IsNullOrWhiteSpace(tailnetHost))
        {
            return builder;
        }

        return builder.WithUrls(context =>
        {
            var tailnetUrls = new List<ResourceUrlAnnotation>();

            foreach (var url in context.Urls)
            {
                if (!Uri.TryCreate(url.Url, UriKind.Absolute, out var uri)
                    || uri.Scheme != Uri.UriSchemeHttps
                    || !uri.IsLoopback
                    || !PublishedPorts.Contains(uri.Port))
                {
                    continue;
                }

                tailnetUrls.Add(new ResourceUrlAnnotation
                {
                    Url = new UriBuilder(uri) { Host = tailnetHost }.Uri.ToString(),
                    // Without display text the dashboard shows the tailnet URL itself.
                    DisplayText = string.IsNullOrEmpty(url.DisplayText) ? null : $"{url.DisplayText} (tailnet)",
                    Endpoint = url.Endpoint,
                    // Show tailnet links first; they are the ones that work from other devices.
                    DisplayOrder = (url.DisplayOrder ?? 0) + 1000
                });
            }

            foreach (var tailnetUrl in tailnetUrls)
            {
                context.Urls.Add(tailnetUrl);
            }
        });
    }
}
