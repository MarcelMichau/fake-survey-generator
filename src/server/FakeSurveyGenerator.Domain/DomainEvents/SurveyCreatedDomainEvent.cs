using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Shared.SeedWork;

namespace FakeSurveyGenerator.Domain.DomainEvents;

public sealed class SurveyCreatedDomainEvent : DomainEvent
{
    public Survey Survey { get; }

    public SurveyCreatedDomainEvent(Survey survey)
    {
        Survey = survey ?? throw new ArgumentNullException(nameof(survey));
    }
}