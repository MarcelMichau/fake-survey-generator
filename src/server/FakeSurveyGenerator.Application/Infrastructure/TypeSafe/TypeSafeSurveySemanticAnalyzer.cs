using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Features.Surveys;
using FakeSurveyGenerator.Application.Shared.Errors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Application.Infrastructure.TypeSafe;

internal sealed class TypeSafeSurveySemanticAnalyzer(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<TypeSafeSurveySemanticAnalyzer> logger)
    : ISurveySemanticAnalyzer
{
    private const double ChoiceWarningThreshold = 0.60;
    private const double LeadingWarningThreshold = 0.70;
    private const double MultipleChoiceWarningThreshold = 0.70;
    private const double DuplicateWarningThreshold = 0.75;
    private const double CoverageWarningThreshold = 0.30;
    private const double FunWarningThreshold = 0.70;
    private const double BikeSheddingWarningThreshold = 0.85;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    private readonly IConfiguration _configuration =
        configuration ?? throw new ArgumentNullException(nameof(configuration));

    private readonly ILogger<TypeSafeSurveySemanticAnalyzer> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result<SurveyAnalysisModel, Error>> AnalyzeAsync(
        AnalyzeSurveyCommand command,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["TYPESAFE_API_KEY"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new Error(
                "typesafe.not.configured",
                "Survey analysis is unavailable because TYPESAFE_API_KEY is not configured.");
        }

        var options = command.SurveyOptions
            .Select((option, index) => new SurveyOptionState(
                index,
                option.OptionText,
                option.PreferredNumberOfVotes))
            .ToList();

        var questions = BuildQuestions(options.Count);
        var request = new TypeSafeRequest
        {
            State = new
            {
                survey = new
                {
                    topic = command.SurveyTopic,
                    audience = command.RespondentType,
                    options
                }
            },
            Model = _configuration["TYPESAFE_MODEL"] ?? "jev-latest",
            Questions = questions
        };

        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "v1/systemone");
            httpRequest.Content = JsonContent.Create(request, options: JsonOptions);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "TypeSafe survey analysis returned HTTP status {StatusCode}: {ResponseBody}",
                    (int)response.StatusCode,
                    responseBody);

                return new Error(
                    "typesafe.request.failed",
                    "Survey analysis is temporarily unavailable. Please try again.");
            }

            var systemOneResponse = JsonSerializer.Deserialize<TypeSafeResponse>(responseBody, JsonOptions);
            if (systemOneResponse?.Answers is null)
            {
                return new Error(
                    "typesafe.invalid_response",
                    "Survey analysis returned an invalid response.");
            }

            return BuildAnalysis(options, systemOneResponse.Answers);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("TypeSafe survey analysis timed out.");
            return new Error(
                "typesafe.timeout",
                "Survey analysis timed out. Please try again.");
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "TypeSafe survey analysis request failed.");
            return new Error(
                "typesafe.request.failed",
                "Survey analysis is temporarily unavailable. Please try again.");
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception, "TypeSafe survey analysis returned malformed JSON.");
            return new Error(
                "typesafe.invalid_response",
                "Survey analysis returned an invalid response.");
        }
    }

    private static Dictionary<string, TypeSafeQuestion> BuildQuestions(int optionCount)
    {
        var questions = new Dictionary<string, TypeSafeQuestion>
        {
            ["response_shape"] = ChoiceQuestion(
                "What response shape does `survey.topic` describe for `survey.audience`?",
                new Dictionary<string, string>
                {
                    ["single_choice"] = "Respondents should select exactly one of the supplied survey options.",
                    ["multiple_choice"] = "Respondents could reasonably select more than one supplied option.",
                    ["rating_or_scale"] =
                        "Respondents should provide a rating, degree, or ordered scale rather than select one named option.",
                    ["open_ended"] =
                        "Respondents should provide free-form text rather than select one supplied option.",
                    ["unclear"] = "The intended response shape cannot be determined from the topic and audience."
                }),
            ["leading"] = NoulQuestion(
                "Is `survey.topic` worded in a way that leads `survey.audience` toward a particular supplied option?",
                "The wording suggests, praises, criticizes, or presupposes a preferred answer.",
                "The wording is neutral, or any preference is merely part of the subject being asked about."),
            ["multiple_choice"] = NoulQuestion(
                "Could a reasonable member of `survey.audience` choose more than one of `survey.options` at the same time?",
                "Two or more supplied options could naturally be true or attractive simultaneously.",
                "The options represent mutually exclusive answers to the topic."),
            ["coverage"] = NoulQuestion(
                "Do `survey.options` cover the main plausible answers to `survey.topic` for `survey.audience`?",
                "The supplied options cover the obvious answer space, with no important missing alternative.",
                "An important plausible answer is missing, or the options do not cover the question's answer space."),
            ["bike_shedding"] = NoulQuestion(
                "Does `survey.topic` ask `survey.audience` to crown a programming language, editor, operating system, framework, or similar technical preference as universally best or worst without naming a task, constraint, or success criterion?",
                "The question invites a context-free technical holy war, such as asking which programming language is best without explaining best for what.",
                "The question supplies a task, constraint, or success criterion, or is not a context-free technical preference debate."),
            ["context_vacuum"] = NoulQuestion(
                "Does `survey.topic` explicitly ask `survey.audience` to choose a best, worst, favourite, or equivalent superlative preference without enough context, criteria, goals, or constraints to make the comparison meaningful? Only return true for a direct or clearly equivalent superlative. Do not treat a yes-or-no question, emphatic language, or a question that merely asks which option to use, choose, or recommend as a context vacuum.",
                "The question explicitly asks respondents to rank or choose a superlative preference without saying what matters or what the answer should be good at.",
                "The question gives respondents meaningful criteria, goals, constraints, or enough context for the comparison, or it does not explicitly ask for a superlative preference."),
            ["audience_plot_twist"] = NoulQuestion(
                "Is there a conspicuous or amusing mismatch between `survey.topic` and `survey.audience`, such that this audience would be an unusually unsuitable group to answer the question?",
                "The audience and question are a clear mismatch, not merely a broad or unconventional survey audience.",
                "The audience can reasonably answer the question, even if the pairing is a little unusual."),
            ["profanity"] = NoulQuestion(
                "Does any of `survey.topic`, `survey.audience`, or `survey.options` contain profanity, vulgar language, or an obscenity, including deliberately censored or obfuscated spellings?",
                "At least one survey field contains profanity, vulgar language, or an obscenity. Do not treat ordinary informal language as profanity.",
                "None of the survey fields contains profanity, vulgar language, or an obscenity."),
        };

        for (var firstIndex = 0; firstIndex < optionCount; firstIndex++)
        {
            for (var secondIndex = firstIndex + 1; secondIndex < optionCount; secondIndex++)
            {
                questions[$"duplicate_{firstIndex}_{secondIndex}"] = NoulQuestion(
                    $"Do `survey.options[{firstIndex}]` and `survey.options[{secondIndex}]` express substantially the same answer?",
                    "The two option texts would mean essentially the same thing to a reasonable respondent, even if their wording differs.",
                    "The two option texts represent meaningfully different answers.");
            }
        }

        return questions;
    }

    private static TypeSafeQuestion ChoiceQuestion(
        string instructions,
        Dictionary<string, string> criteria)
    {
        return new TypeSafeQuestion
        {
            Type = "choice",
            Instructions = instructions,
            Criteria = criteria
        };
    }

    private static TypeSafeQuestion NoulQuestion(
        string instructions,
        string trueCriteria,
        string falseCriteria)
    {
        return new TypeSafeQuestion
        {
            Type = "noul",
            Instructions = instructions,
            Criteria = new Dictionary<string, string>
            {
                ["true"] = trueCriteria,
                ["false"] = falseCriteria
            }
        };
    }

    private static SurveyAnalysisModel BuildAnalysis(
        IReadOnlyList<SurveyOptionState> options,
        IReadOnlyDictionary<string, JsonElement> answers)
    {
        var responseShapeAnswer = GetAnswer(answers, "response_shape");
        var responseShape = GetString(responseShapeAnswer, "choice");
        var responseShapeConfidence = GetDouble(responseShapeAnswer, "confidence");
        var leadingProbability = GetNoul(answers, "leading");
        var multipleChoiceProbability = GetNoul(answers, "multiple_choice");
        var coverageProbability = GetNoul(answers, "coverage");
        var bikeSheddingProbability = GetNoul(answers, "bike_shedding");
        var contextVacuumProbability = GetNoul(answers, "context_vacuum");
        var audiencePlotTwistProbability = GetNoul(answers, "audience_plot_twist");
        var profanityProbability = GetNoul(answers, "profanity");
        var warnings = new List<SurveyAnalysisWarningModel>();

        if (responseShape != "single_choice" && responseShapeConfidence >= ChoiceWarningThreshold)
        {
            warnings.Add(new SurveyAnalysisWarningModel
            {
                Code = "response.shape",
                Title = "Radio Buttons Cannot Contain This",
                Message =
                    $"This question has outgrown a single-choice survey; it looks more like a {FormatResponseShape(responseShape)} question.",
                Probability = responseShapeConfidence
            });
        }

        if (leadingProbability >= LeadingWarningThreshold)
        {
            warnings.Add(new SurveyAnalysisWarningModel
            {
                Code = "question.leading",
                Title = "Survey Has Already Voted",
                Message = "This question appears to have a favourite. Try not to make respondents agree under oath.",
                Probability = leadingProbability
            });
        }

        if (multipleChoiceProbability >= MultipleChoiceWarningThreshold)
        {
            warnings.Add(new SurveyAnalysisWarningModel
            {
                Code = "options.multiple",
                Title = "Commitment Issues",
                Message = "A reasonable human may want several of these. Radio buttons are feeling restrictive.",
                Probability = multipleChoiceProbability
            });
        }

        if (coverageProbability <= CoverageWarningThreshold)
        {
            warnings.Add(new SurveyAnalysisWarningModel
            {
                Code = "options.coverage",
                Title = "Answer Buffet Is Missing a Course",
                Message = "There may be perfectly reasonable answers outside this tiny, suspiciously curated menu.",
                Probability = 1 - coverageProbability
            });
        }

        if (bikeSheddingProbability >= BikeSheddingWarningThreshold)
        {
            warnings.Add(new SurveyAnalysisWarningModel
            {
                Code = "question.bike_shedding",
                Title = "Bike Shedding Detected",
                Message = "You have begun a timeless debate with no success criteria. Please specify the job, or prepare for 800 comments.",
                Probability = bikeSheddingProbability
            });
        }

        if (bikeSheddingProbability < BikeSheddingWarningThreshold && contextVacuumProbability >= FunWarningThreshold)
        {
            warnings.Add(new SurveyAnalysisWarningModel
            {
                Code = "question.context_vacuum",
                Title = "Context Vacuum",
                Message = "Best at what? Please provide a goal, a constraint, or at least a vibe.",
                Probability = contextVacuumProbability
            });
        }

        if (audiencePlotTwistProbability >= FunWarningThreshold)
        {
            warnings.Add(new SurveyAnalysisWarningModel
            {
                Code = "audience.plot_twist",
                Title = "Audience–Question Plot Twist",
                Message = "Asking this audience that question is bold. Possibly inspired.",
                Probability = audiencePlotTwistProbability
            });
        }

        if (profanityProbability >= FunWarningThreshold)
        {
            warnings.Add(new SurveyAnalysisWarningModel
            {
                Code = "content.profanity",
                Title = "Foul Mouth",
                Message = "One or more survey fields contain language that would make a sailor blush.",
                Probability = profanityProbability
            });
        }

        if (options.Any(option => option.PreferredNumberOfVotes > 0))
        {
            warnings.Add(new SurveyAnalysisWarningModel
            {
                Code = "results.rigged",
                Title = "Rigging Advisory",
                Message = "The results have been pre-seasoned. Statistical integrity has left the chat.",
                Probability = 1
            });
        }

        for (var firstIndex = 0; firstIndex < options.Count; firstIndex++)
        {
            for (var secondIndex = firstIndex + 1; secondIndex < options.Count; secondIndex++)
            {
                var questionId = $"duplicate_{firstIndex}_{secondIndex}";
                var duplicateProbability = GetNoul(answers, questionId);
                if (duplicateProbability < DuplicateWarningThreshold)
                {
                    continue;
                }

                warnings.Add(new SurveyAnalysisWarningModel
                {
                    Code = $"options.duplicate.{firstIndex}.{secondIndex}",
                    Title = "Two Hats, One Answer",
                    Message =
                        $"\"{options[firstIndex].Text}\" and \"{options[secondIndex].Text}\" may express the same answer.",
                    Probability = duplicateProbability
                });
            }
        }

        return new SurveyAnalysisModel
        {
            ResponseShape = responseShape,
            ResponseShapeConfidence = responseShapeConfidence,
            LeadingProbability = leadingProbability,
            MultipleChoiceProbability = multipleChoiceProbability,
            CoverageProbability = coverageProbability,
            Warnings = warnings
        };
    }

    private static JsonElement GetAnswer(
        IReadOnlyDictionary<string, JsonElement> answers,
        string questionId)
    {
        return answers.TryGetValue(questionId, out var answer)
            ? answer
            : throw new JsonException($"TypeSafe response did not contain answer '{questionId}'.");
    }

    private static string GetString(JsonElement answer, string propertyName)
    {
        if (!answer.TryGetProperty(propertyName, out var property))
        {
            throw new JsonException($"TypeSafe answer did not contain property '{propertyName}'.");
        }

        if (property.ValueKind != JsonValueKind.String)
        {
            throw new JsonException($"TypeSafe answer property '{propertyName}' was not a string.");
        }

        return property.GetString()!;
    }

    private static double GetDouble(JsonElement answer, string propertyName)
    {
        if (!answer.TryGetProperty(propertyName, out var property))
        {
            throw new JsonException($"TypeSafe answer did not contain property '{propertyName}'.");
        }

        if (property.ValueKind != JsonValueKind.Number || !property.TryGetDouble(out var value))
        {
            throw new JsonException($"TypeSafe answer property '{propertyName}' was not a number.");
        }

        return value;
    }

    private static double GetNoul(
        IReadOnlyDictionary<string, JsonElement> answers,
        string questionId)
    {
        return GetDouble(GetAnswer(answers, questionId), "noul");
    }

    private static string FormatResponseShape(string responseShape)
    {
        return responseShape switch
        {
            "multiple_choice" => "multiple-choice",
            "rating_or_scale" => "rating or scale",
            "open_ended" => "open-ended",
            _ => "unclear"
        };
    }

    private sealed record SurveyOptionState(int Id, string Text, int PreferredNumberOfVotes);

    private sealed class TypeSafeRequest
    {
        public required object State { get; init; }
        public required string Model { get; init; }
        public required Dictionary<string, TypeSafeQuestion> Questions { get; init; }
    }

    private sealed class TypeSafeQuestion
    {
        public required string Type { get; init; }
        public required string Instructions { get; init; }
        public required object Criteria { get; init; }
    }

    private sealed class TypeSafeResponse
    {
        [JsonPropertyName("answers")] public Dictionary<string, JsonElement>? Answers { get; init; }
    }
}
