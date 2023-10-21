using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Api.Data;

internal sealed class DatabaseCreationHostedService(IServiceProvider serviceProvider, IHostEnvironment hostEnvironment,
        ILogger<DatabaseCreationHostedService> logger)
    : IHostedService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
    private readonly ILogger<DatabaseCreationHostedService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_hostEnvironment.IsDevelopment()) // Only migrate database on startup when running in Development environment
        {
            _logger.LogInformation("Skipping database creation/migration as application is not running in Development environment");
            return;
        }

        var scope = _serviceProvider.CreateScope();

        await using var context = scope.ServiceProvider.GetRequiredService<SurveyContext>();

        if (context.Database.IsSqlServer()) // Do not migrate database when running integration tests with in-memory database
        {
            _logger.LogInformation("Creating/Migrating Database...");

            try
            {
                await context.Database.MigrateAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while migrating the database");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}