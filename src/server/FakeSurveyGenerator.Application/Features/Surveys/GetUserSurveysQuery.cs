using CSharpFunctionalExtensions;
using Dapper;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Errors;
using FakeSurveyGenerator.Application.Shared.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Features.Surveys;

public sealed record GetUserSurveysQuery : IRequest<Result<List<UserSurveyModel>, Error>>;

public sealed class
    GetUserSurveysQueryHandler(
        IUserService userService,
        SurveyContext surveyContext)
    : IRequestHandler<GetUserSurveysQuery, Result<List<UserSurveyModel>, Error>>
{
    private readonly SurveyContext _surveyContext =
        surveyContext ?? throw new ArgumentNullException(nameof(surveyContext));

    private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

    public async Task<Result<List<UserSurveyModel>, Error>> Handle(GetUserSurveysQuery request,
        CancellationToken cancellationToken)
    {
        var userInfo = await _userService.GetUserInfo(cancellationToken);

        var surveyOwner =
            await _surveyContext.Users.FirstAsync(user => user.ExternalUserId == userInfo.Id,
                cancellationToken);

        var connection = _surveyContext.Database.GetDbConnection();

        await connection.OpenAsync(cancellationToken);

        var surveys = await connection.QueryAsync<UserSurveyModel>(@"
                    SELECT s.Id,
                           s.Topic,
                           s.RespondentType,
                           s.NumberOfRespondents,
                      (SELECT COUNT(*)
                       FROM [Survey].[SurveyOption]
                       WHERE SurveyId = s.Id) AS NumberOfOptions,
                           surveyOption1.OptionText AS WinningOption,
                           surveyOption1.NumberOfVotes AS WinningOptionNumberOfVotes
                    FROM [Survey].[Survey] s
                    LEFT OUTER JOIN Survey.SurveyOption surveyOption1 ON surveyOption1.SurveyId = s.Id
                    LEFT OUTER JOIN Survey.SurveyOption surveyOption2 ON surveyOption2.SurveyId = s.Id
                    AND surveyOption2.NumberOfVotes > surveyOption1.NumberOfVotes
                    WHERE s.OwnerId = @ownerId
                      AND surveyOption2.SurveyId IS NULL
                    GROUP BY s.Id,
                             s.Topic,
                             s.RespondentType,
                             s.NumberOfRespondents,
                             s.CreatedOn,
                             surveyOption1.OptionText,
                             surveyOption1.NumberOfVotes
                    ORDER BY s.CreatedOn DESC
                    ", new { ownerId = surveyOwner.Id });

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