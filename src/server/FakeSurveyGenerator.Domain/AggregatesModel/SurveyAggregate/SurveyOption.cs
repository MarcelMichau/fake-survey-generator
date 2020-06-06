using FakeSurveyGenerator.Domain.Common;
using FakeSurveyGenerator.Domain.SeedWork;

namespace FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate
{
    public class SurveyOption : Entity
    {
        public NonEmptyString OptionText { get; }
        public int NumberOfVotes { get; private set; }
        public int PreferredNumberOfVotes { get; }

        public SurveyOption(NonEmptyString optionText)
        {
            OptionText = optionText;
            PreferredNumberOfVotes = 0;
        }

        public SurveyOption(NonEmptyString optionText, int preferredNumberOfVotes)
        {
            OptionText = optionText;
            PreferredNumberOfVotes = preferredNumberOfVotes;
        }

        internal void AddVote()
        {
            NumberOfVotes++;
        }
    }
}
