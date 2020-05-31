using System;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using MediatR;

namespace FakeSurveyGenerator.Domain.DomainEvents
{
    public class SurveyCreatedDomainEvent : INotification
    {
        public Survey Survey { get; }

        public SurveyCreatedDomainEvent(Survey survey)
        {
            Survey = survey ?? throw new ArgumentNullException(nameof(survey));
        }
    }
}
