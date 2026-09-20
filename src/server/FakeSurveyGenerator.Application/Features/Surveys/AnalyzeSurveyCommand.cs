using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Abstractions;
using FakeSurveyGenerator.Application.Shared.Errors;
using FluentValidation;

namespace FakeSurveyGenerator.Application.Features.Surveys;

public sealed record AnalyzeSurveyCommand : ICommand<Result<SurveyAnalysisModel, Error>>
{
    public required string SurveyTopic { get; init; }
    public required string RespondentType { get; init; }
    public required IEnumerable<SurveyOptionDto> SurveyOptions { get; init; } = [];
}

public sealed record SurveyAnalysisWarningModel
{
    public required string Code { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public required double Probability { get; init; }
}

public sealed record SurveyAnalysisModel
{
    public required string ResponseShape { get; init; }
    public required double ResponseShapeConfidence { get; init; }
    public required double LeadingProbability { get; init; }
    public required double MultipleChoiceProbability { get; init; }
    public required double CoverageProbability { get; init; }
    public required List<SurveyAnalysisWarningModel> Warnings { get; init; }
}

public interface ISurveySemanticAnalyzer
{
    Task<Result<SurveyAnalysisModel, Error>> AnalyzeAsync(
        AnalyzeSurveyCommand command,
        CancellationToken cancellationToken = default);
}

public sealed class AnalyzeSurveyCommandValidator : AbstractValidator<AnalyzeSurveyCommand>
{
    private const int MaximumOptionsForAnalysis = 20;

    public AnalyzeSurveyCommandValidator()
    {
        RuleFor(command => command.SurveyTopic)
            .MaximumLength(250)
            .NotEmpty();

        RuleFor(command => command.RespondentType)
            .MaximumLength(250)
            .NotEmpty();

        RuleFor(command => command.SurveyOptions)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(options => options is not null && options.Count() <= MaximumOptionsForAnalysis)
            .WithMessage($"A maximum of {MaximumOptionsForAnalysis} options can be analyzed at once.");

        RuleForEach(command => command.SurveyOptions)
            .SetValidator(new SurveyOptionValidator());
    }
}

public sealed class AnalyzeSurveyCommandHandler(
    ISurveySemanticAnalyzer analyzer,
    IValidator<AnalyzeSurveyCommand> validator)
    : ICommandHandler<AnalyzeSurveyCommand, Result<SurveyAnalysisModel, Error>>
{
    private readonly ISurveySemanticAnalyzer _analyzer =
        analyzer ?? throw new ArgumentNullException(nameof(analyzer));

    private readonly IValidator<AnalyzeSurveyCommand> _validator =
        validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<Result<SurveyAnalysisModel, Error>> Handle(
        AnalyzeSurveyCommand request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Errors.General.ValidationError(validationResult);
        }

        return await _analyzer.AnalyzeAsync(request, cancellationToken);
    }
}
