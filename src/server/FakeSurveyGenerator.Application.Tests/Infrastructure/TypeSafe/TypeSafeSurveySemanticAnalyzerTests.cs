using System.Net;
using System.Text;
using System.Text.Json;
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
        var response = JsonSerializer.Serialize(new
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
        return CreateAnalyzer(new StaticResponseHandler(response));
    }

    private static TypeSafeSurveySemanticAnalyzer CreateAnalyzer(
        HttpMessageHandler handler,
        string? apiKey = "test-key")
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
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(response, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class ExceptionResponseHandler(Exception exception) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromException<HttpResponseMessage>(exception);
        }
    }
}
