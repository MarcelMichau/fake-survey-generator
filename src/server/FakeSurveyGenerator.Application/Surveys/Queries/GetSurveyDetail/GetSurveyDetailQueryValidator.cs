using FluentValidation;

namespace FakeSurveyGenerator.Application.Surveys.Queries.GetSurveyDetail
{
    public class GetSurveyDetailQueryValidator : AbstractValidator<GetSurveyDetailQuery>
    {
        public GetSurveyDetailQueryValidator()
        {
            RuleFor(request => request.Id).GreaterThan(0);
        }
    }
}
