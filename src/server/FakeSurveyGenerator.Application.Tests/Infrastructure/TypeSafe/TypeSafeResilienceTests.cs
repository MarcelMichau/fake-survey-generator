using System.Net;
using FakeSurveyGenerator.Application.Features.Surveys;
using FakeSurveyGenerator.Application.Infrastructure.TypeSafe;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Application.Tests.Infrastructure.TypeSafe;

public sealed class TypeSafeResilienceTests
{
    [Test]
    public async Task GivenInheritedResilience_WhenProviderFails_ThenPostIsSentOnlyOnce()
    {
        using var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));
        using var host = CreateHost(handler);

        var result = await host.Services.GetRequiredService<ISurveySemanticAnalyzer>().AnalyzeAsync(Command());

        await Assert.That(result.Error.Code).IsEqualTo("typesafe.request.failed");
        await Assert.That(handler.RequestCount).IsEqualTo(1);
    }

    [Test]
    public async Task GivenSlowProvider_WhenAttemptTimesOut_ThenReturnsTimeoutWithoutRetrying()
    {
        using var handler = new RecordingHandler(WaitForCancellation);
        using var host = CreateHost(handler, new Dictionary<string, string?>
        {
            ["TypeSafe:Resilience:AttemptTimeout:Timeout"] = "00:00:01",
            ["TypeSafe:Resilience:TotalRequestTimeout:Timeout"] = "00:00:05",
            ["TypeSafe:Resilience:CircuitBreaker:SamplingDuration"] = "00:00:02"
        });

        var result = await host.Services.GetRequiredService<ISurveySemanticAnalyzer>().AnalyzeAsync(Command());

        await Assert.That(result.Error.Code).IsEqualTo("typesafe.timeout");
        await Assert.That(handler.RequestCount).IsEqualTo(1);
    }

    [Test]
    public async Task GivenStalledResponseBody_WhenTotalTimeoutExpires_ThenReturnsTimeoutWithoutRetrying()
    {
        using var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StalledContent()
        }));
        using var host = CreateHost(handler, new Dictionary<string, string?>
        {
            ["TypeSafe:Resilience:AttemptTimeout:Timeout"] = "00:00:01",
            ["TypeSafe:Resilience:TotalRequestTimeout:Timeout"] = "00:00:01",
            ["TypeSafe:Resilience:CircuitBreaker:SamplingDuration"] = "00:00:02"
        });

        var result = await host.Services.GetRequiredService<ISurveySemanticAnalyzer>().AnalyzeAsync(Command());

        await Assert.That(result.Error.Code).IsEqualTo("typesafe.timeout");
        await Assert.That(handler.RequestCount).IsEqualTo(1);
    }

    [Test]
    public async Task GivenCallerCancellation_WhenAnalyzing_ThenCancellationPropagates()
    {
        using var handler = new RecordingHandler(WaitForCancellation);
        using var host = CreateHost(handler);
        using var cancellation = new CancellationTokenSource();
        var analysis = host.Services.GetRequiredService<ISurveySemanticAnalyzer>().AnalyzeAsync(Command(), cancellation.Token);
        await handler.Started.Task;
        await cancellation.CancelAsync();

        await Assert.That(async () => await analysis).Throws<OperationCanceledException>();
        await Assert.That(handler.RequestCount).IsEqualTo(1);
    }

    [Test]
    public async Task GivenFailedProvider_WhenCircuitOpens_ThenFurtherCallsAreRejected()
    {
        using var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));
        using var host = CreateHost(handler, new Dictionary<string, string?>
        {
            ["TypeSafe:Resilience:CircuitBreaker:MinimumThroughput"] = "2",
            ["TypeSafe:Resilience:CircuitBreaker:FailureRatio"] = "0.5"
        });
        var analyzer = host.Services.GetRequiredService<ISurveySemanticAnalyzer>();
        await analyzer.AnalyzeAsync(Command());
        await analyzer.AnalyzeAsync(Command());

        var result = await analyzer.AnalyzeAsync(Command());

        await Assert.That(result.Error.Code).IsEqualTo("typesafe.unavailable");
        await Assert.That(handler.RequestCount).IsEqualTo(2);
    }

    [Test]
    public async Task GivenConcurrencyLimitReached_WhenAnalyzing_ThenReturnsBusyWithoutCallingProvider()
    {
        using var handler = new RecordingHandler(WaitForCancellation);
        using var host = CreateHost(handler, new Dictionary<string, string?>
        {
            ["TypeSafe:Resilience:RateLimiter:DefaultRateLimiterOptions:PermitLimit"] = "1"
        });
        var analyzer = host.Services.GetRequiredService<ISurveySemanticAnalyzer>();
        using var cancellation = new CancellationTokenSource();
        var inFlight = analyzer.AnalyzeAsync(Command(), cancellation.Token);
        await handler.Started.Task;

        var result = await analyzer.AnalyzeAsync(Command());
        await cancellation.CancelAsync();
        await Assert.That(async () => await inFlight).Throws<OperationCanceledException>();

        await Assert.That(result.Error.Code).IsEqualTo("typesafe.busy");
        await Assert.That(handler.RequestCount).IsEqualTo(1);
    }

    private static IHost CreateHost(HttpMessageHandler handler, Dictionary<string, string?>? settings = null)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["TYPESAFE_API_KEY"] = "test-key" });
        if (settings is not null) builder.Configuration.AddInMemoryCollection(settings);
        // Exercise removal of the inherited default, as configured by ServiceDefaults.
        builder.Services.ConfigureHttpClientDefaults(http => http.AddStandardResilienceHandler());
        builder.AddTypeSafeConfiguration();
        builder.Services.AddHttpClient<ISurveySemanticAnalyzer, TypeSafeSurveySemanticAnalyzer>()
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        return builder.Build();
    }

    private static AnalyzeSurveyCommand Command() => new()
    {
        SurveyTopic = "Tabs or spaces?",
        RespondentType = "Developers",
        SurveyOptions = [new SurveyOptionDto { OptionText = "Tabs" }, new SurveyOptionDto { OptionText = "Spaces" }]
    };

    private static async Task<HttpResponseMessage> WaitForCancellation(HttpRequestMessage _, CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        return new HttpResponseMessage(HttpStatusCode.OK);
    }

    private sealed class StalledContent : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            throw new NotSupportedException();

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken) =>
            Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }

    private sealed class RecordingHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        private int _requestCount;
        public int RequestCount => _requestCount;
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _requestCount);
            Started.TrySetResult();
            return send(request, cancellationToken);
        }
    }
}
