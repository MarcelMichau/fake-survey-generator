using System.Linq;
using FluentValidation;

namespace FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey
{
    public class CreateSurveyCommandValidator : AbstractValidator<CreateSurveyCommand>
    {
        public CreateSurveyCommandValidator()
        {
            RuleFor(command => command.SurveyTopic)
                .MaximumLength(250)
                .WithMessage("{PropertyName} should have a maximum length of {MaxLength}")
                .NotEmpty()
                .WithMessage("{PropertyName} should not be empty");

            RuleFor(command => command.RespondentType)
                .MaximumLength(250)
                .WithMessage("{PropertyName} should have a maximum length of {MaxLength}")
                .NotEmpty()
                .WithMessage("{PropertyName} should not be empty");

            RuleFor(command => command.NumberOfRespondents)
                .GreaterThan(0)
                .WithMessage("{PropertyName} should be greater than {ComparisonValue}");

            RuleFor(command => command.SurveyOptions)
                .Must(collection => collection != null && collection.Any())
                .WithMessage("{PropertyName} should have at least one item");

            RuleForEach(command => command.SurveyOptions)
                .SetValidator(new SurveyOptionValidator());
        }
    }

    public class SurveyOptionValidator : AbstractValidator<SurveyOptionDto>
    {
        public SurveyOptionValidator()
        {
            RuleFor(command => command.OptionText)
                .MaximumLength(250)
                .WithMessage("{PropertyName} should have a maximum length of {MaxLength}")
                .NotEmpty()
                .WithMessage("{PropertyName} should not be empty");
        }
    }
}
