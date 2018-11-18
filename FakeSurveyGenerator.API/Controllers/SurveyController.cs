using System.Threading;
using System.Threading.Tasks;
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
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var topic = "To be or not to be";
            var numberOfRespondents = 1500;
            var respondentType = "Late Victorian Prostitutes";

            var survey = new Survey(topic, numberOfRespondents, respondentType);

            survey.AddSurveyOption("Yes", 1);
            survey.AddSurveyOption("No", 2);
            survey.AddSurveyOption("Maybe", 3);
            survey.AddSurveyOption("I didn't understand the question", 4);

            var voteDistributionStrategy = new RandomVoteDistributionStrategy();

            var result = survey.CalculateOutcome(voteDistributionStrategy);

            var insertedSurvey = _surveyRepository.Add(result);

            await _surveyRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(insertedSurvey);
        }
    }
}