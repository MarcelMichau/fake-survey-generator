using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.Identity;
using FakeSurveyGenerator.Application.Features.Surveys;
using FakeSurveyGenerator.Application.Infrastructure.TypeSafe;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace FakeSurveyGenerator.Application.Tests.Infrastructure.TypeSafe;

public sealed class TypeSafeSurveySemanticAnalyzerTests
{
    [Test]
    public async Task GivenTypeSafeReportsProfanityForAContextFreeTechDebateAndRiggedVotes_WhenAnalyzing_ThenReturnsFunWarnings()
    {
        var analyzer = CreateAnalyzer(bikeSheddingProbability: 0.9, contextVacuumProbability: 0.9,
            audiencePlotTwistProbability: 0.8, profanityProbability: 0.9);

        var result = await analyzer.AnalyzeAsync(CreateCommand());

        await Assert.That(result.IsSuccess).IsTrue();
        var warnings = result.Value.Warnings;
        await Assert.That(warnings.Select(warning => warning.Title)).IsEquivalentTo(
        [
            "Survey Has Already Voted",
            "Commitment Issues",
            "Answer Buffet Is Missing a Course",
            "Bike Shedding Detected",
            "Audience–Question Plot Twist",
            "Foul Mouth",
            "Rigging Advisory",
            "Two Hats, One Answer"
        ]);
        await Assert.That(warnings.Any(warning => warning.Code == "question.context_vacuum")).IsFalse();
    }

    [Test]
    public async Task GivenAContextFreeQuestionThatIsNotABikeShed_WhenAnalyzing_ThenReturnsContextVacuum()
    {
        var analyzer = CreateAnalyzer(bikeSheddingProbability: 0.1, contextVacuumProbability: 0.9,
            audiencePlotTwistProbability: 0.1, profanityProbability: 0.1);

        var result = await analyzer.AnalyzeAsync(CreateCommand());

        await Assert.That(result.IsSuccess).IsTrue();
        var warning = result.Value.Warnings.Single(warning => warning.Code == "question.context_vacuum");
        await Assert.That(warning.Title).IsEqualTo("Context Vacuum");
        await Assert.That(warning.Message).IsEqualTo(
            "Best at what? Please provide a goal, a constraint, or at least a vibe.");
    }

    [Test]
    public async Task GivenAWeakBikeSheddingSignal_WhenAnalyzing_ThenDoesNotRaiseBikeShedding()
    {
        var analyzer = CreateAnalyzer(bikeSheddingProbability: 0.84, contextVacuumProbability: 0.1,
            audiencePlotTwistProbability: 0.1, profanityProbability: 0.1);

        var result = await analyzer.AnalyzeAsync(CreateCommand());

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.Warnings.Any(warning => warning.Code == "question.bike_shedding")).IsFalse();
    }

    [Test]
    public async Task GivenMissingApiKey_WhenAnalyzing_ThenReturnsNotConfiguredErrorWithoutCallingProvider()
    {
        var analyzer = CreateAnalyzer(new ExceptionResponseHandler(new HttpRequestException()), apiKey: null);

        var result = await analyzer.AnalyzeAsync(CreateCommand());

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error.Code).IsEqualTo("typesafe.not.configured");
    }

    [Test]
    public async Task GivenAuthorizationProviderToken_WhenAnalyzing_ThenSendsItAsBearerToken()
    {
        var handler = new StaticResponseHandler(ValidResponse());
        var analyzer = CreateAnalyzer(handler, authorizationProvider: new StaticAuthorizationProvider("entra-token"));

        var result = await analyzer.AnalyzeAsync(CreateCommand());

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(handler.LastAuthorization?.Scheme).IsEqualTo("Bearer");
        await Assert.That(handler.LastAuthorization?.Parameter).IsEqualTo("entra-token");
    }

    [Test]
    public async Task GivenASurvey_WhenAnalyzing_ThenSendsJsonBodyWithContentLength()
    {
        var handler = new StaticResponseHandler(ValidResponse());
        var analyzer = CreateAnalyzer(handler);

        await analyzer.AnalyzeAsync(CreateCommand());

        // Microsoft Foundry rejects chunked request bodies.
        await Assert.That(handler.LastContentLength).IsNotNull();
        await Assert.That(handler.LastContentType).IsEqualTo("application/json");
    }

    [Test]
    public async Task GivenAzureCredentialCannotBeObtained_WhenAnalyzing_ThenReturnsAuthenticationFailedErrorWithoutCallingProvider()
    {
        var handler = new ExceptionResponseHandler(new HttpRequestException());
        var analyzer = CreateAnalyzer(handler, authorizationProvider: new StaticAuthorizationProvider(
            exception: new CredentialUnavailableException("No developer credential is signed in.")));

        var result = await analyzer.AnalyzeAsync(CreateCommand());

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error.Code).IsEqualTo("typesafe.authentication.failed");
        await Assert.That(handler.RequestCount).IsEqualTo(0);
    }

    [Test]
    public async Task GivenProviderReturnsFailureStatus_WhenAnalyzing_ThenReturnsRequestFailedError()
    {
        var analyzer = CreateAnalyzer(new StaticResponseHandler("provider unavailable", HttpStatusCode.ServiceUnavailable));

        var result = await analyzer.AnalyzeAsync(CreateCommand());

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error.Code).IsEqualTo("typesafe.request.failed");
    }

    [Test]
    public async Task GivenMalformedProviderJson_WhenAnalyzing_ThenReturnsInvalidResponseError()
    {
        var analyzer = CreateAnalyzer(new StaticResponseHandler("{not-json"));

        var result = await analyzer.AnalyzeAsync(CreateCommand());

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error.Code).IsEqualTo("typesafe.invalid_response");
    }

    [Test]
    public async Task GivenProviderResponseWithoutAnswers_WhenAnalyzing_ThenReturnsInvalidResponseError()
    {
        var analyzer = CreateAnalyzer(new StaticResponseHandler("{}"));

        var result = await analyzer.AnalyzeAsync(CreateCommand());

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error.Code).IsEqualTo("typesafe.invalid_response");
    }

    [Test]
    [Arguments("""{"answers":{"response_shape":null}}""")]
    [Arguments("""{"answers":{"response_shape":"single_choice"}}""")]
    [Arguments("""{"answers":{"response_shape":[1,2,3]}}""")]
    [Arguments("""{"answers":{"response_shape":{"choice":"single_choice","confidence":"high"}}}""")]
    [Arguments("""{"answers":{"response_shape":{"choice":7,"confidence":1.0}}}""")]
    public async Task GivenAnswerWithUnexpectedShape_WhenAnalyzing_ThenReturnsInvalidResponseError(string responseBody)
    {
        var analyzer = CreateAnalyzer(new StaticResponseHandler(responseBody));

        var result = await analyzer.AnalyzeAsync(CreateCommand());

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error.Code).IsEqualTo("typesafe.invalid_response");
    }

    [Test]
    public async Task GivenProviderRequestFails_WhenAnalyzing_ThenReturnsRequestFailedError()
    {
        var analyzer = CreateAnalyzer(new ExceptionResponseHandler(new HttpRequestException("network failure")));

        var result = await analyzer.AnalyzeAsync(CreateCommand());

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error.Code).IsEqualTo("typesafe.request.failed");
    }

    [Test]
    public async Task GivenProviderRequestTimesOut_WhenAnalyzing_ThenReturnsTimeoutError()
    {
        var analyzer = CreateAnalyzer(new ExceptionResponseHandler(new OperationCanceledException("request timed out")));

        var result = await analyzer.AnalyzeAsync(CreateCommand());

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error.Code).IsEqualTo("typesafe.timeout");
    }

    private static TypeSafeSurveySemanticAnalyzer CreateAnalyzer(
        double bikeSheddingProbability,
        double contextVacuumProbability,
        double audiencePlotTwistProbability,
        double profanityProbability)
    {
        return CreateAnalyzer(new StaticResponseHandler(ValidResponse(bikeSheddingProbability, contextVacuumProbability,
            audiencePlotTwistProbability, profanityProbability)));
    }

    private static string ValidResponse(
        double bikeSheddingProbability = 0.1,
        double contextVacuumProbability = 0.1,
        double audiencePlotTwistProbability = 0.1,
        double profanityProbability = 0.1)
    {
        return JsonSerializer.Serialize(new
        {
            answers = new
            {
                response_shape = new { choice = "single_choice", confidence = 1.0 },
                leading = new { noul = 0.9 },
                multiple_choice = new { noul = 0.9 },
                coverage = new { noul = 0.1 },
                bike_shedding = new { noul = bikeSheddingProbability },
                context_vacuum = new { noul = contextVacuumProbability },
                audience_plot_twist = new { noul = audiencePlotTwistProbability },
                profanity = new { noul = profanityProbability },
                duplicate_0_1 = new { noul = 0.9 }
            }
        });
    }

    private static TypeSafeSurveySemanticAnalyzer CreateAnalyzer(
        HttpMessageHandler handler,
        string? apiKey = "test-key",
        ITypeSafeAuthorizationProvider? authorizationProvider = null)
    {
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://typesafe.test/")
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["TYPESAFE_API_KEY"] = apiKey })
            .Build();

        return new TypeSafeSurveySemanticAnalyzer(
            client,
            authorizationProvider ?? new ApiKeyTypeSafeAuthorizationProvider(configuration),
            configuration,
            NullLogger<TypeSafeSurveySemanticAnalyzer>.Instance);
    }

    private static AnalyzeSurveyCommand CreateCommand() => new()
    {
        SurveyTopic = "Which programming language is the best?",
        RespondentType = "Overcaffeinated developers",
        SurveyOptions =
        [
            new SurveyOptionDto { OptionText = "Absolutely", PreferredNumberOfVotes = 3 },
            new SurveyOptionDto { OptionText = "Obviously" }
        ]
    };

    private sealed class StaticResponseHandler(string response, HttpStatusCode statusCode = HttpStatusCode.OK)
        : HttpMessageHandler
    {
        public AuthenticationHeaderValue? LastAuthorization { get; private set; }
        public long? LastContentLength { get; private set; }
        public string? LastContentType { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastAuthorization = request.Headers.Authorization;
            LastContentLength = request.Content?.Headers.ContentLength;
            LastContentType = request.Content?.Headers.ContentType?.MediaType;
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(response, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class ExceptionResponseHandler(Exception exception) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            return Task.FromException<HttpResponseMessage>(exception);
        }
    }

    private sealed class StaticAuthorizationProvider(string? token = null, Exception? exception = null)
        : ITypeSafeAuthorizationProvider
    {
        public ValueTask<string?> GetBearerTokenAsync(CancellationToken cancellationToken) =>
            exception is null ? ValueTask.FromResult(token) : ValueTask.FromException<string?>(exception);
    }
}
