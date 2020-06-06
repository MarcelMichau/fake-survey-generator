using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FakeSurveyGenerator.Application.Common.Interfaces;
using FakeSurveyGenerator.Application.Surveys.Models;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.Common;
using MediatR;

namespace FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey
{
    public sealed class CreateSurveyCommandHandler : IRequestHandler<CreateSurveyCommand, SurveyModel>
    {
        private readonly ISurveyContext _surveyContext;
        private readonly IMapper _mapper;

        public CreateSurveyCommandHandler(ISurveyContext surveyContext, IMapper mapper)
        {
            _surveyContext = surveyContext ?? throw new ArgumentNullException(nameof(surveyContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<SurveyModel> Handle(CreateSurveyCommand request, CancellationToken cancellationToken)
        {
            var survey = new Survey(NonEmptyString.Create(request.SurveyTopic), request.NumberOfRespondents, NonEmptyString.Create(request.RespondentType));

            survey.AddSurveyOptions(request.SurveyOptions.Select(option => new SurveyOption(NonEmptyString.Create(option.OptionText), option.PreferredNumberOfVotes)));

            survey.CalculateOutcome();

            await _surveyContext.Surveys.AddAsync(survey, cancellationToken);

            await _surveyContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<SurveyModel>(survey);
        }
    }
}
