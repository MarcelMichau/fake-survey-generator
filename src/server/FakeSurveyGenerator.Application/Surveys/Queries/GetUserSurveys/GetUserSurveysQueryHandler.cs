using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Dapper;
using FakeSurveyGenerator.Application.Common.Errors;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Application.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Surveys.Queries.GetUserSurveys
{
    public sealed class
        GetUserSurveysQueryHandler : IRequestHandler<GetUserSurveysQuery, Result<List<UserSurveyModel>, Error>>
    {
        private readonly IUserService _userService;
        private readonly ISurveyContext _surveyContext;
        private readonly IDatabaseConnection _databaseConnection;

        public GetUserSurveysQueryHandler(IDatabaseConnection databaseConnection, IUserService userService,
            ISurveyContext surveyContext)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _surveyContext = surveyContext ?? throw new ArgumentNullException(nameof(surveyContext));
            _databaseConnection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));
        }

        public async Task<Result<List<UserSurveyModel>, Error>> Handle(GetUserSurveysQuery request,
            CancellationToken cancellationToken)
        {
            var userInfo = await _userService.GetUserInfo(cancellationToken);

            var surveyOwner =
                await _surveyContext.Users.FirstAsync(user => user.ExternalUserId == userInfo.Id,
                    cancellationToken);

            await using var connection = await _databaseConnection.GetDbConnection();
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
                                 surveyOption1.OptionText,
                                 surveyOption1.NumberOfVotes
                        ", new {ownerId = surveyOwner.Id});

            return Result.Success<List<UserSurveyModel>, Error>(surveys.ToList());
        }
    }
}