using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.Services;

namespace FakeSurveyGenerator.API.Application.Commands
{
    public class CreateSurveyCommandHandler : IRequestHandler<CreateSurveyCommand, Survey>
    {
        private readonly ISurveyRepository _surveyRepository;

        public CreateSurveyCommandHandler(ISurveyRepository surveyRepository)
        {
            _surveyRepository = surveyRepository;
        }

        public async Task<Survey> Handle(CreateSurveyCommand message, CancellationToken cancellationToken)
        {
            var survey = new Survey(message.SurveyTopic, message.NumberOfRespondents, message.RespondentType);

            foreach (var option in message.SurveyOptions)
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

            return insertedSurvey;
        }
    }
}
