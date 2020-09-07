namespace FakeSurveyGenerator.Application.Surveys.Queries.GetUserSurveys
{
    public sealed class UserSurveyModel
    {
        public int Id { get; set; }
        public string Topic { get; set; }
        public string RespondentType { get; set; }
        public int NumberOfRespondents { get; set; }
        public int NumberOfOptions { get; set; }
        public string WinningOption { get; set; }
        public int WinningOptionNumberOfVotes { get; set; }
    }
}
