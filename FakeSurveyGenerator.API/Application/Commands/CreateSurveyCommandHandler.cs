using System.Linq;
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

        public async Task<SurveyModel> Handle(CreateSurveyCommand request, CancellationToken cancellationToken)
        {
            var survey = new Survey(request.SurveyTopic, request.NumberOfRespondents, request.RespondentType);

            foreach (var option in request.SurveyOptions)
            {
                if (option.PreferredOutcomeRank.HasValue)
                    survey.AddSurveyOption(option.OptionText, option.PreferredOutcomeRank.Value);
                else
                    survey.AddSurveyOption(option.OptionText);
            }

            IVoteDistribution voteDistribution;

            if (request.SurveyOptions.Any(option => option.PreferredOutcomeRank > 0))
                voteDistribution = new RankedVoteDistribution();
            else
                voteDistribution = new RandomVoteDistribution();

            var result = survey.CalculateOutcome(voteDistribution);

            var insertedSurvey = _surveyRepository.Add(result);

            await _surveyRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<SurveyModel>(insertedSurvey);
        }
    }
}
