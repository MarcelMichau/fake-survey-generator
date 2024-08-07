using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Worker;

internal sealed class Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
    : BackgroundService
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IServiceScopeFactory _serviceScopeFactory =
        serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(10));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _timer.WaitForNextTickAsync(stoppingToken))
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

    private async Task GetTotalSurveys(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var surveyContext = scope.ServiceProvider.GetRequiredService<SurveyContext>();

        var surveyCount = await surveyContext.Surveys.CountAsync(stoppingToken);

        _logger.LogInformation("Current Number of Surveys in Database: {SurveyCount}", surveyCount);
    }

    public override void Dispose()
    {
        _timer.Dispose();
        base.Dispose();
    }
}