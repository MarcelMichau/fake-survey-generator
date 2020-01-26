using System;
using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Infrastructure;
using Microsoft.EntityFrameworkCore;
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

            var surveyContext = scope.ServiceProvider.GetService<SurveyContext>();

            var surveyCount = await surveyContext.Surveys.CountAsync(stoppingToken);

            _logger.LogInformation($"Current Number of Surveys in Database: {surveyCount}");
        }
    }
}
