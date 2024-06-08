using FakeSurveyGenerator.Application.Domain.Surveys;
using FakeSurveyGenerator.Application.Shared.Auditing;

namespace FakeSurveyGenerator.Application.Features.Surveys;

public sealed record SurveyModel : AuditableModel
{
    public required int Id { get; init; }
    public required int OwnerId { get; init; }
    public required string Topic { get; init; }
    public required string RespondentType { get; init; }
    public required int NumberOfRespondents { get; init; }
    public required bool IsRigged { get; init; }
    public required List<SurveyOptionModel> Options { get; init; }
}

public static class SurveyModelMappingExtensions
{
    public static SurveyModel MapToModel(this Survey survey)
    {
        return new SurveyModel
        {
            Id = survey.Id,
            OwnerId = survey.Owner.Id,
            Topic = survey.Topic.Value,
            RespondentType = survey.RespondentType.Value,
            NumberOfRespondents = survey.NumberOfRespondents,
            IsRigged = survey.IsRigged,
            Options = survey.Options.Select(option => option.MapToModel()).ToList(),
            CreatedBy = survey.CreatedBy,
            CreatedOn = survey.CreatedOn,
            ModifiedBy = survey.ModifiedBy,
            ModifiedOn = survey.ModifiedOn
        };
    }
}