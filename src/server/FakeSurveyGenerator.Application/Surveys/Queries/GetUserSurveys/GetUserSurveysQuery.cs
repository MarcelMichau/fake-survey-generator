using System.Collections.Generic;
using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Errors;
using MediatR;

namespace FakeSurveyGenerator.Application.Surveys.Queries.GetUserSurveys
{
    public sealed class GetUserSurveysQuery : IRequest<Result<List<UserSurveyModel>, Error>>
    {
    }
}
