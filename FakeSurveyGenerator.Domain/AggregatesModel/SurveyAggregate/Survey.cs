using System;
using System.Collections.Generic;
using System.Linq;
using FakeSurveyGenerator.Domain.Exceptions;
using FakeSurveyGenerator.Domain.SeedWork;

namespace FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate
{
    public class Survey : Entity, IAggregateRoot
    {
        public Survey(string topic, int numberOfRespondents, string respondentType)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new SurveyDomainException("Survey topic cannot be empty");

            if (numberOfRespondents < 1)
                throw new SurveyDomainException("Survey should have at least one respondent");

            if (string.IsNullOrWhiteSpace(respondentType))
                throw new SurveyDomainException("Type of respondent cannot be empty");

            Topic = topic;
            RespondentType = respondentType;
            NumberOfRespondents = numberOfRespondents;
            CreatedOn = DateTime.UtcNow;
            _options = new List<SurveyOption>();
        }

        public string Topic { get; }
        public string RespondentType { get; }
        public int NumberOfRespondents { get; }
        public DateTime CreatedOn { get; }

        private readonly List<SurveyOption> _options;

        public IReadOnlyCollection<SurveyOption> Options => _options;

        public void AddSurveyOption(string optionText)
        {
            var newOption = new SurveyOption(optionText);

            _options.Add(newOption);
        }

        public Survey CalculateResult()
        {
            if (!_options.Any())
                throw new SurveyDomainException("Cannot calculate a survey with no options");

            var random = new Random();

            for (var i = 0; i < NumberOfRespondents; i++)
            {
                var randomIndex = random.Next(0, _options.Count);
                _options[randomIndex].AddVote();
            }

            return this;
        }
    }
}
