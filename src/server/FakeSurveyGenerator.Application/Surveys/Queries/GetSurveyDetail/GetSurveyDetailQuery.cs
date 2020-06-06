using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Errors;
using FakeSurveyGenerator.Application.Surveys.Models;
using MediatR;

namespace FakeSurveyGenerator.Application.Surveys.Queries.GetSurveyDetail
{
    public sealed class GetSurveyDetailQuery : IRequest<Result<SurveyModel, Error>>
    {
        public int Id { get; set; }

        public GetSurveyDetailQuery(int id)
        {
            Id = id;
        }
    }
}
