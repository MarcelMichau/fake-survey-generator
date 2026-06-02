using CSharpFunctionalExtensions;
using Dapper;
using FakeSurveyGenerator.Application.Abstractions;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Errors;
using FakeSurveyGenerator.Application.Shared.Identity;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Features.Surveys;

public sealed record GetUserSurveysQuery : IQuery<Result<List<UserSurveyModel>, Error>>;

public sealed class
    GetUserSurveysQueryHandler(
        IUserService userService,
        SurveyContext surveyContext)
    : IQueryHandler<GetUserSurveysQuery, Result<List<UserSurveyModel>, Error>>
{
    private readonly SurveyContext _surveyContext =
        surveyContext ?? throw new ArgumentNullException(nameof(surveyContext));

    private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

    public async Task<Result<List<UserSurveyModel>, Error>> Handle(GetUserSurveysQuery request,
        CancellationToken cancellationToken = default)
    {
        var userInfo = await _userService.GetUserInfo(cancellationToken);

        var connection = _surveyContext.Database.GetDbConnection();
        var command = new CommandDefinition(
            """
            SELECT s.Id,
                   s.Topic,
                   s.RespondentType,
                   s.NumberOfRespondents,
                   COALESCE(optionCount.NumberOfOptions, 0) AS NumberOfOptions,
                   winner.OptionText AS WinningOption,
                   winner.NumberOfVotes AS WinningOptionNumberOfVotes
            FROM [Survey].[Survey] s
            INNER JOIN [Survey].[User] u ON u.Id = s.OwnerId
            LEFT JOIN (
                SELECT so.SurveyId,
                       COUNT(*) AS NumberOfOptions
                FROM [Survey].[SurveyOption] so
                GROUP BY so.SurveyId
            ) optionCount ON optionCount.SurveyId = s.Id
            OUTER APPLY (
                SELECT TOP (1)
                       so.OptionText,
                       so.NumberOfVotes
                FROM [Survey].[SurveyOption] so
                WHERE so.SurveyId = s.Id
                ORDER BY so.NumberOfVotes DESC, so.Id ASC
            ) winner
            WHERE u.ExternalUserId = @externalUserId
            ORDER BY s.CreatedOn DESC
            """,
            new { externalUserId = userInfo.Id },
            cancellationToken: cancellationToken);

        var surveys = await connection.QueryAsync<UserSurveyModel>(command);

        return surveys.ToList();
    }
}

public sealed record UserSurveyModel
{
    public required int Id { get; init; }
    public required string Topic { get; init; }
    public required string RespondentType { get; init; }
    public required int NumberOfRespondents { get; init; }
    public required int NumberOfOptions { get; init; }
    public required string WinningOption { get; init; }
    public required int WinningOptionNumberOfVotes { get; init; }
}