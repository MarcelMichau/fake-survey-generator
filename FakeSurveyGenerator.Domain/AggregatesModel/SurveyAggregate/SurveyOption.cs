using FakeSurveyGenerator.Domain.Exceptions;
using FakeSurveyGenerator.Domain.SeedWork;

namespace FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate
{
    public class SurveyOption : Entity
    {
        public SurveyOption(string optionText)
        {
            if (string.IsNullOrWhiteSpace(optionText))
                throw new SurveyDomainException("Survey Option cannot be empty");

            OptionText = optionText;
            PreferredOutcomeRank = 0;
        }

        public SurveyOption(string optionText, int preferredOutcomeRank)
        {
            if (string.IsNullOrWhiteSpace(optionText))
                throw new SurveyDomainException("Survey Option cannot be empty");

            OptionText = optionText;
            PreferredOutcomeRank = preferredOutcomeRank;
        }

        public string OptionText { get; }
        public int NumberOfVotes { get; private set; }
        public int PreferredOutcomeRank { get; }

        internal void AddVote()
        {
            NumberOfVotes++;
        }
    }
}
