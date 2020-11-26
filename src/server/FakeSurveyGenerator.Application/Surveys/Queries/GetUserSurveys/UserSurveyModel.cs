namespace FakeSurveyGenerator.Application.Surveys.Queries.GetUserSurveys
{
    public sealed class UserSurveyModel
    {
        public int Id { get; init; }
        public string Topic { get; init; }
        public string RespondentType { get; init; }
        public int NumberOfRespondents { get; init; }
        public int NumberOfOptions { get; init; }
        public string WinningOption { get; init; }
        public int WinningOptionNumberOfVotes { get; init; }
    }
}
