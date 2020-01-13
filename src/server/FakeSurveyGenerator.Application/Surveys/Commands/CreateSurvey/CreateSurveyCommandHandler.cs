using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FakeSurveyGenerator.Application.Surveys.Models;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.Services;
using MediatR;

namespace FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey
{
    public class CreateSurveyCommandHandler : IRequestHandler<CreateSurveyCommand, SurveyModel>
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreateSurveyCommandHandler(ISurveyRepository surveyRepository, IMapper mapper, IMediator mediator)
        {
            _surveyRepository = surveyRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<SurveyModel> Handle(CreateSurveyCommand request, CancellationToken cancellationToken)
        {
            var survey = new Survey(request.SurveyTopic, request.NumberOfRespondents, request.RespondentType);

            foreach (var option in request.SurveyOptions)
            {
                if (option.PreferredNumberOfVotes.HasValue)
                    survey.AddSurveyOption(option.OptionText, option.PreferredNumberOfVotes.Value);
                else
                    survey.AddSurveyOption(option.OptionText);
            }

            IVoteDistribution voteDistribution;

            if (request.SurveyOptions.Any(option => option.PreferredNumberOfVotes > 0))
                voteDistribution = new FixedVoteDistribution();
            else
                voteDistribution = new RandomVoteDistribution();

            var result = survey.CalculateOutcome(voteDistribution);

            _surveyRepository.Add(result);

            await _surveyRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new SurveyCreated(result.Id), cancellationToken);

            return _mapper.Map<SurveyModel>(result);
        }
    }
}
