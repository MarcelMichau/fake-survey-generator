using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FakeSurveyGenerator.Application.Common.Interfaces;
using FakeSurveyGenerator.Application.Surveys.Models;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using MediatR;

namespace FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey
{
    public class CreateSurveyCommandHandler : IRequestHandler<CreateSurveyCommand, SurveyModel>
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
            var survey = new Survey(request.SurveyTopic, request.NumberOfRespondents, request.RespondentType);

            survey.AddSurveyOptions(request.SurveyOptions.Select(option => new SurveyOption(option.OptionText, option.PreferredNumberOfVotes)));

            var result = survey.CalculateOutcome();

            await _surveyContext.Surveys.AddAsync(result, cancellationToken);

            await _surveyContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<SurveyModel>(result);
        }
    }
}
