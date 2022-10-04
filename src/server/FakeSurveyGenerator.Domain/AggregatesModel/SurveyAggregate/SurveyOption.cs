using FakeSurveyGenerator.Domain.Common;
using FakeSurveyGenerator.Shared.SeedWork;
using JetBrains.Annotations;

namespace FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

public class SurveyOption : AuditableEntity
{
    public NonEmptyString OptionText { get; }
    public int NumberOfVotes { get; private set; }
    public int PreferredNumberOfVotes { get; }

    [UsedImplicitly]
    private SurveyOption() { } // Necessary for Entity Framework Core + AutoMapper

    public SurveyOption(NonEmptyString optionText)
    {
        OptionText = optionText;
        PreferredNumberOfVotes = 0;
    }

    public SurveyOption(NonEmptyString optionText, int preferredNumberOfVotes)
    {
        OptionText = optionText ?? throw new ArgumentNullException(nameof(optionText));
        PreferredNumberOfVotes = preferredNumberOfVotes;
    }

    internal void AddVote()
    {
        NumberOfVotes++;
    }
}