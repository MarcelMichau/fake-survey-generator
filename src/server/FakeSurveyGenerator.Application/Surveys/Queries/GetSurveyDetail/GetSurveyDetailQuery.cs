using FakeSurveyGenerator.Application.Surveys.Models;
using MediatR;

namespace FakeSurveyGenerator.Application.Surveys.Queries.GetSurveyDetail
{
    public class GetSurveyDetailQuery : IRequest<SurveyModel>
    {
        public int Id { get; set; }

        public GetSurveyDetailQuery(int id)
        {
            Id = id;
        }
    }
}
