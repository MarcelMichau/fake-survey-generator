using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.API.Application;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace FakeSurveyGenerator.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SurveyController : Controller
    {
        private readonly ISurveyRepository _surveyRepository;

        public SurveyController(ISurveyRepository surveyRepository)
        {
            _surveyRepository = surveyRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] CreateSurveyCommand command, CancellationToken cancellationToken)
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

            return Ok(insertedSurvey);
        }
    }
}