using FakeSurveyGenerator.Application.EventBus;

namespace FakeSurveyGenerator.Application.Domain.Surveys;

public sealed class SurveyCreatedDomainEvent(Survey survey) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.Now;
    public Survey Survey { get; } = survey ?? throw new ArgumentNullException(nameof(survey));
}