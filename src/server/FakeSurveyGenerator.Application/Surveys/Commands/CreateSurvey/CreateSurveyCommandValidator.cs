using FluentValidation;

namespace FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey
{
    public sealed class CreateSurveyCommandValidator : AbstractValidator<CreateSurveyCommand>
    {
        public CreateSurveyCommandValidator()
        {
            RuleFor(command => command.SurveyTopic)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .MaximumLength(250)
                .NotEmpty();

            RuleFor(command => command.RespondentType)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .MaximumLength(250)
                .NotEmpty();

            RuleFor(command => command.NumberOfRespondents)
                .GreaterThan(0);

            RuleFor(command => command.SurveyOptions)
                .NotEmpty();

            RuleForEach(command => command.SurveyOptions)
                .SetValidator(new SurveyOptionValidator());
        }
    }

    public sealed class SurveyOptionValidator : AbstractValidator<SurveyOptionDto>
    {
        public SurveyOptionValidator()
        {
            RuleFor(command => command.OptionText)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .MaximumLength(250)
                .NotEmpty();
        }
    }
}
