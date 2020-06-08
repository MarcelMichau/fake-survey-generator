using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Errors;
using FakeSurveyGenerator.Application.Common.Interfaces;
using FakeSurveyGenerator.Application.Surveys.Models;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.Common;
using FakeSurveyGenerator.Domain.Exceptions;
using MediatR;

namespace FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey
{
    public sealed class CreateSurveyCommandHandler : IRequestHandler<CreateSurveyCommand, Result<SurveyModel, Error>>
    {
        private readonly ISurveyContext _surveyContext;
        private readonly IMapper _mapper;

        public CreateSurveyCommandHandler(ISurveyContext surveyContext, IMapper mapper)
        {
            _surveyContext = surveyContext ?? throw new ArgumentNullException(nameof(surveyContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<Result<SurveyModel, Error>> Handle(CreateSurveyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var survey = new Survey(NonEmptyString.Create(request.SurveyTopic), request.NumberOfRespondents, NonEmptyString.Create(request.RespondentType));

                survey.AddSurveyOptions(request.SurveyOptions.Select(option => new SurveyOption(NonEmptyString.Create(option.OptionText), option.PreferredNumberOfVotes)));

                survey.CalculateOutcome();

                await _surveyContext.Surveys.AddAsync(survey, cancellationToken);

                await _surveyContext.SaveChangesAsync(cancellationToken);

                return Result.Success<SurveyModel, Error>(_mapper.Map<SurveyModel>(survey));
            }
            catch (SurveyDomainException e)
            {
                return Result.Failure<SurveyModel, Error>(new Error("survey.domain.exception", e.Message));
            }
        }
    }
}
