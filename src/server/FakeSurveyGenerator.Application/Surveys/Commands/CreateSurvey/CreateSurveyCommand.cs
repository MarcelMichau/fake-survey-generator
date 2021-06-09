using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Errors;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Application.Common.Persistence;
using FakeSurveyGenerator.Application.Surveys.Models;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.Common;
using FakeSurveyGenerator.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey
{
    public sealed record CreateSurveyCommand : IRequest<Result<SurveyModel, Error>>
    {
        public string SurveyTopic { get; init; }

        public int NumberOfRespondents { get; init; }

        public string RespondentType { get; init; }

        public IEnumerable<SurveyOptionDto> SurveyOptions { get; init; }

        public CreateSurveyCommand()
        {
             SurveyOptions = new List<SurveyOptionDto>();
        }

        public CreateSurveyCommand(string surveyTopic, int numberOfRespondents, string respondentType, IEnumerable<SurveyOptionDto> surveyOptions) : this()
        {
            SurveyTopic = surveyTopic;
            NumberOfRespondents = numberOfRespondents;
            RespondentType = respondentType;
            SurveyOptions = surveyOptions;
        }
    }

    public sealed record SurveyOptionDto
    {
        public string OptionText { get; init; }
        public int PreferredNumberOfVotes { get; init; }
    }

    public sealed class CreateSurveyCommandHandler : IRequestHandler<CreateSurveyCommand, Result<SurveyModel, Error>>
    {
        private readonly ISurveyContext _surveyContext;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public CreateSurveyCommandHandler(ISurveyContext surveyContext, IMapper mapper, IUserService userService)
        {
            _surveyContext = surveyContext ?? throw new ArgumentNullException(nameof(surveyContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<Result<SurveyModel, Error>> Handle(CreateSurveyCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var userInfo = await _userService.GetUserInfo(cancellationToken);

                var surveyOwner =
                    await _surveyContext.Users.FirstAsync(user => user.ExternalUserId == userInfo.Id,
                        cancellationToken);

                var survey = new Survey(surveyOwner, NonEmptyString.Create(request.SurveyTopic),
                    request.NumberOfRespondents, NonEmptyString.Create(request.RespondentType));

                survey.AddSurveyOptions(request.SurveyOptions.Select(option =>
                    new SurveyOption(NonEmptyString.Create(option.OptionText), option.PreferredNumberOfVotes)));

                survey.CalculateOutcome();

                await _surveyContext.Surveys.AddAsync(survey, cancellationToken);

                await _surveyContext.SaveChangesAsync(cancellationToken);

                return _mapper.Map<SurveyModel>(survey);
            }
            catch (SurveyDomainException e)
            {
                return new Error("survey.domain.exception", e.Message);
            }
        }
    }
}
