using System;
using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Worker;

internal sealed class Worker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(10));

    public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await GetTotalSurveys(stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Oh no! Something bad happened.");
            }
        }
    }

    private async Task GetTotalSurveys(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var surveyContext = scope.ServiceProvider.GetRequiredService<SurveyContext>();

        var surveyCount = await surveyContext.Surveys.CountAsync(stoppingToken);

        _logger.LogInformation($"Current Number of Surveys in Database: {surveyCount}");
    }
}