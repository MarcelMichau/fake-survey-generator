﻿using System.Text.Json;
using FakeSurveyGenerator.Application.Domain.Shared;
using FakeSurveyGenerator.Application.Domain.Shared.SeedWork;
using FakeSurveyGenerator.Application.Domain.Surveys.VoteDistributions;
using FakeSurveyGenerator.Application.Domain.Users;
using JetBrains.Annotations;

namespace FakeSurveyGenerator.Application.Domain.Surveys;

public sealed class Survey : AuditableEntity, IAggregateRoot
{
    private readonly List<SurveyOption> _options = [];

    private IVoteDistribution _selectedVoteDistribution = new RandomVoteDistribution();

    [UsedImplicitly]
    private Survey()
    {
    } // Necessary for Entity Framework Core

    public Survey(User owner, NonEmptyString topic, int numberOfRespondents, NonEmptyString respondentType)
    {
        if (numberOfRespondents < 1)
            throw new SurveyDomainException("Survey should have at least one respondent");

        Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        Topic = topic ?? throw new ArgumentNullException(nameof(topic));
        RespondentType = respondentType ?? throw new ArgumentNullException(nameof(respondentType));
        NumberOfRespondents = numberOfRespondents;

        _selectedVoteDistribution = new RandomVoteDistribution();

        AddDomainEvent(new SurveyCreatedDomainEvent(this));
    }

    public User Owner { get; } = null!;
    public NonEmptyString Topic { get; } = null!;
    public NonEmptyString RespondentType { get; } = null!;
    public int NumberOfRespondents { get; }
    public IReadOnlyList<SurveyOption> Options => _options.AsReadOnly();
    public bool IsRigged => _options.Any(option => option.IsRigged);

    public void AddSurveyOption(NonEmptyString optionText)
    {
        ThrowIfDuplicateOptions(optionText);

        var newOption = new SurveyOption(optionText);

        _options.Add(newOption);
    }

    public void AddSurveyOption(NonEmptyString optionText, int preferredNumberOfVotes)
    {
        ThrowIfDuplicateOptions(optionText);
        
        if (preferredNumberOfVotes > NumberOfRespondents ||
            _options.Sum(option => option.PreferredNumberOfVotes) + preferredNumberOfVotes > NumberOfRespondents)
            throw new SurveyDomainException(
                $"Preferred number of votes: {preferredNumberOfVotes} is higher than the number of respondents: {NumberOfRespondents}");

        var newOption = new SurveyOption(optionText, preferredNumberOfVotes);

        _options.Add(newOption);
    }

    public void AddSurveyOptions(IEnumerable<SurveyOption> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        foreach (var surveyOption in options)
            AddSurveyOption(surveyOption.OptionText, surveyOption.PreferredNumberOfVotes);
    }

    public void CalculateOutcome()
    {
        ThrowIfNoOptions();
        DetermineVoteDistributionStrategy();

        _selectedVoteDistribution.DistributeVotes(this);
    }

    public void CalculateOneSidedOutcome()
    {
        ThrowIfNoOptions();
        _selectedVoteDistribution = new OneSidedVoteDistribution();

        _selectedVoteDistribution.DistributeVotes(this);
    }

    private void ThrowIfNoOptions()
    {
        if (_options.Count == 0)
            throw new SurveyDomainException($"Cannot calculate the outcome of a Survey with no Options for Survey with Topic: {Topic}");
    }
    
    private void ThrowIfDuplicateOptions(NonEmptyString optionText)
    {
        if (_options.Any(o => string.Equals(o.OptionText, optionText, StringComparison.OrdinalIgnoreCase)))
            throw new SurveyDomainException("Duplicate survey option.");
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
        return JsonSerializer.Serialize(this);
    }
}