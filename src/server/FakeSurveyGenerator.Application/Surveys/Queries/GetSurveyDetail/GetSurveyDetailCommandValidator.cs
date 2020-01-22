using FluentValidation;

namespace FakeSurveyGenerator.Application.Surveys.Queries.GetSurveyDetail
{
    public class GetSurveyDetailCommandValidator : AbstractValidator<GetSurveyDetailQuery>
    {
        public GetSurveyDetailCommandValidator()
        {
            RuleFor(request => request.Id).GreaterThan(0);
        }
    }
}
