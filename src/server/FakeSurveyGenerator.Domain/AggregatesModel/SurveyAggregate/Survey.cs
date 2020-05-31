using System;
using System.Collections.Generic;
using System.Linq;
using FakeSurveyGenerator.Domain.DomainEvents;
using FakeSurveyGenerator.Domain.Exceptions;
using FakeSurveyGenerator.Domain.SeedWork;
using FakeSurveyGenerator.Domain.Services;

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

            _selectedVoteDistribution = new RandomVoteDistribution();

            AddSurveyCreatedEvent();
        }

        public string Topic { get; }
        public string RespondentType { get; }
        public int NumberOfRespondents { get; }
        public DateTime CreatedOn { get; }

        private readonly List<SurveyOption> _options;

        public IReadOnlyList<SurveyOption> Options => _options;

        private IVoteDistribution _selectedVoteDistribution;

        public void AddSurveyOption(string optionText)
        {
            var newOption = new SurveyOption(optionText);

            _options.Add(newOption);
        }

        public void AddSurveyOption(string optionText, int preferredNumberOfVotes)
        {
            if (preferredNumberOfVotes > NumberOfRespondents || _options.Sum(option => option.PreferredNumberOfVotes) + preferredNumberOfVotes > NumberOfRespondents)
                throw new SurveyDomainException($"Preferred number of votes: {preferredNumberOfVotes} is higher than the number of respondents: {NumberOfRespondents}");

            var newOption = new SurveyOption(optionText, preferredNumberOfVotes);

            _options.Add(newOption);
        }

        public void AddSurveyOptions(IEnumerable<SurveyOption> options)
        {
            foreach (var surveyOption in options)
            {
                AddSurveyOption(surveyOption.OptionText, surveyOption.PreferredNumberOfVotes);
            }
        }

        public Survey CalculateOutcome()
        {
            CheckForZeroOptions();
            DetermineVoteDistributionStrategy();

            _selectedVoteDistribution.DistributeVotes(this);

            return this;
        }

        public Survey CalculateOneSidedOutcome()
        {
            CheckForZeroOptions();
            _selectedVoteDistribution = new OneSidedVoteDistribution();

            _selectedVoteDistribution.DistributeVotes(this);

            return this;
        }

        private void CheckForZeroOptions()
        {
            if (!_options.Any())
                throw new SurveyDomainException("Cannot calculate a survey with no options");
        }

        private void AddSurveyCreatedEvent()
        {
            var surveyCreatedEvent = new SurveyCreatedDomainEvent(this);
            AddDomainEvent(surveyCreatedEvent);
        }

        private void DetermineVoteDistributionStrategy()
        {
            if (_options.Any(option => option.PreferredNumberOfVotes > 0))
                _selectedVoteDistribution = new FixedVoteDistribution();
            else
                _selectedVoteDistribution = new RandomVoteDistribution();
        }

        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }
    }
}
