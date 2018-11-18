using MediatR;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FakeSurveyGenerator.API.Application.Models;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.Services;

namespace FakeSurveyGenerator.API.Application.Commands
{
    public class CreateSurveyCommandHandler : IRequestHandler<CreateSurveyCommand, SurveyModel>
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly IMapper _mapper;

        public CreateSurveyCommandHandler(ISurveyRepository surveyRepository, IMapper mapper)
        {
            _surveyRepository = surveyRepository;
            _mapper = mapper;
        }

        public async Task<SurveyModel> Handle(CreateSurveyCommand command, CancellationToken cancellationToken)
        {
            var survey = new Survey(command.SurveyTopic, command.NumberOfRespondents, command.RespondentType);

            foreach (var option in command.SurveyOptions)
            {
                if (option.PreferredOutcomeRank.HasValue)
                    survey.AddSurveyOption(option.OptionText, option.PreferredOutcomeRank.Value);
                else
                    survey.AddSurveyOption(option.OptionText);
            }

            var voteDistributionStrategy = new RandomVoteDistributionStrategy();

            var result = survey.CalculateOutcome(voteDistributionStrategy);

            var insertedSurvey = _surveyRepository.Add(result);

            await _surveyRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<SurveyModel>(insertedSurvey);
        }
    }
}
