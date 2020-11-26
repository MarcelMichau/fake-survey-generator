﻿using System;
using System.Collections.Generic;
using System.Linq;
using FakeSurveyGenerator.Domain.AggregatesModel.UserAggregate;
using FakeSurveyGenerator.Domain.Common;
using FakeSurveyGenerator.Domain.DomainEvents;
using FakeSurveyGenerator.Domain.Exceptions;
using FakeSurveyGenerator.Domain.Services;
using FakeSurveyGenerator.Shared.SeedWork;

namespace FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate
{
    public sealed class Survey : AuditableEntity, IAggregateRoot
    {
        public User Owner { get; }
        public NonEmptyString Topic { get; }
        public NonEmptyString RespondentType { get; }
        public int NumberOfRespondents { get; }

        private readonly List<SurveyOption> _options = new();
        public IReadOnlyList<SurveyOption> Options => _options.ToList();

        private IVoteDistribution _selectedVoteDistribution;

        private Survey() { } // Necessary for Entity Framework Core

        public Survey(User owner, NonEmptyString topic, int numberOfRespondents, NonEmptyString respondentType)
        {
            if (numberOfRespondents < 1)
                throw new SurveyDomainException("Survey should have at least one respondent");

            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Topic = topic ?? throw new ArgumentNullException(nameof(topic));
            RespondentType = respondentType ?? throw new ArgumentNullException(nameof(respondentType));
            NumberOfRespondents = numberOfRespondents;

            _selectedVoteDistribution = new RandomVoteDistribution();

            AddSurveyCreatedEvent();
        }

        public void AddSurveyOption(NonEmptyString optionText)
        {
            var newOption = new SurveyOption(optionText);

            _options.Add(newOption);
        }

        public void AddSurveyOption(NonEmptyString optionText, int preferredNumberOfVotes)
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

        public void CalculateOutcome()
        {
            CheckForZeroOptions();
            DetermineVoteDistributionStrategy();

            _selectedVoteDistribution.DistributeVotes(this);
        }

        public void CalculateOneSidedOutcome()
        {
            CheckForZeroOptions();
            _selectedVoteDistribution = new OneSidedVoteDistribution();

            _selectedVoteDistribution.DistributeVotes(this);
        }

        private void CheckForZeroOptions()
        {
            if (!_options.Any())
                throw new SurveyDomainException("Cannot calculate the outcome of a Survey with no Options");
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
