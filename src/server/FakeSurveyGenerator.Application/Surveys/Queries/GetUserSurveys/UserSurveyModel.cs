namespace FakeSurveyGenerator.Application.Surveys.Queries.GetUserSurveys;

public sealed record UserSurveyModel
{
    public int Id { get; init; }
    public string Topic { get; init; } = null!;
    public string RespondentType { get; init; } = null!;
    public int NumberOfRespondents { get; init; }
    public int NumberOfOptions { get; init; }
    public string WinningOption { get; init; } = null!;
    public int WinningOptionNumberOfVotes { get; init; }
}