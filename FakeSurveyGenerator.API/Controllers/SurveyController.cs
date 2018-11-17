using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
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
            var topic = "Chosen UI framework";
            var numberOfRespondents = 500;
            var respondentType = "Plebs";

            var survey = new Survey(topic, numberOfRespondents, respondentType);

            survey.AddSurveyOption("Angular");
            survey.AddSurveyOption("React");
            survey.AddSurveyOption("Vue");

            var result = survey.CalculateOutcome();

            var insertedSurvey = _surveyRepository.Add(result);

            await _surveyRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(insertedSurvey);
        }
    }
}