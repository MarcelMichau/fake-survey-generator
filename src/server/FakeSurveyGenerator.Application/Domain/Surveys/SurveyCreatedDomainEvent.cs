using FakeSurveyGenerator.Application.Domain.Shared.SeedWork;

namespace FakeSurveyGenerator.Application.Domain.Surveys;

public sealed class SurveyCreatedDomainEvent(Survey survey) : DomainEvent
{
    public Survey Survey { get; } = survey ?? throw new ArgumentNullException(nameof(survey));
}