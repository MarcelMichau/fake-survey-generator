namespace FakeSurveyGenerator.Application.Infrastructure.TypeSafe;

/// <summary>
/// Buffers provider responses inside the resilience pipeline so its policies cover the complete request.
/// </summary>
internal sealed class TypeSafeResponseBufferingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        try
        {
            await response.Content.LoadIntoBufferAsync(cancellationToken);
            return response;
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }
}
