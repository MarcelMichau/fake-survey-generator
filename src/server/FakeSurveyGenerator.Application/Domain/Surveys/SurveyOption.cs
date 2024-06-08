using FakeSurveyGenerator.Application.Domain.Shared;
using JetBrains.Annotations;

namespace FakeSurveyGenerator.Application.Domain.Surveys;

public class SurveyOption
{
    public NonEmptyString OptionText { get; } = null!;
    public int NumberOfVotes { get; private set; }
    public int PreferredNumberOfVotes { get; }
    public bool IsRigged => PreferredNumberOfVotes > 0;

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