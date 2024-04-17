using FakeSurveyGenerator.Application.Domain.Surveys;
using FakeSurveyGenerator.Application.Shared.Auditing;

namespace FakeSurveyGenerator.Application.Features.Surveys;

public sealed record SurveyModel : AuditableModel
{
    public int Id { get; init; }
    public int OwnerId { get; init; }
    public string Topic { get; init; } = null!;
    public string RespondentType { get; init; } = null!;
    public int NumberOfRespondents { get; init; }
    public List<SurveyOptionModel> Options { get; init; } = null!;
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
            Options = survey.Options.Select(option => option.MapToModel()).ToList(),
            CreatedBy = survey.CreatedBy,
            CreatedOn = survey.CreatedOn,
            ModifiedBy = survey.ModifiedBy,
            ModifiedOn = survey.ModifiedOn
        };
    }
}