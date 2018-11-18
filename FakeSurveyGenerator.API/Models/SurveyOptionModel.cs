namespace FakeSurveyGenerator.API.Models
{
    public class SurveyOptionModel
    {
        public int Id { get; set; }
        public string OptionText { get; set; }
        public int NumberOfVotes { get; set; }
        public int PreferredOutcomeRank { get; set; }
    }
}
