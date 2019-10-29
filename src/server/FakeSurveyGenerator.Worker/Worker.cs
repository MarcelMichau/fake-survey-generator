using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await CreateSurvey(stoppingToken);
                await Task.Delay(5000, stoppingToken);
            }
        }

        private async Task CreateSurvey(CancellationToken stoppingToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var surveyRepository = scope.ServiceProvider.GetService<ISurveyRepository>();

            var survey = new Survey("Test Topic", 100, "Testers");

            survey.AddSurveyOption("Option 1");
            survey.AddSurveyOption("Option 2");
            survey.AddSurveyOption("Option 3");
            survey.AddSurveyOption("Option 4");

            IVoteDistribution voteDistribution = new RandomVoteDistribution();

            var result = survey.CalculateOutcome(voteDistribution);

            surveyRepository.Add(result);

            await surveyRepository.UnitOfWork.SaveChangesAsync(stoppingToken);

            var winningOption = result.Options.OrderByDescending(option => option.NumberOfVotes).First();

            _logger.LogInformation($"Added Survey with topic: {result.Topic} asking: {result.NumberOfRespondents} {result.RespondentType}. Winning option: {winningOption.OptionText} with: {winningOption.NumberOfVotes} votes");
        }
    }
}
