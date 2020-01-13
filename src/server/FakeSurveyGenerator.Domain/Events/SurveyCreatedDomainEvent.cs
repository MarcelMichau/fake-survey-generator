using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using MediatR;

namespace FakeSurveyGenerator.Domain.Events
{
    public class SurveyCreatedDomainEvent : INotification
    {
        public Survey Survey { get; }

        public SurveyCreatedDomainEvent(Survey survey)
        {
            Survey = survey;
        }
    }
}
