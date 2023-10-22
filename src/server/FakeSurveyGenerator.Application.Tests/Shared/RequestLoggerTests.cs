using FakeSurveyGenerator.Application.Features.Surveys;
using FakeSurveyGenerator.Application.Shared.Behaviours;
using FakeSurveyGenerator.Application.Shared.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FakeSurveyGenerator.Application.Tests.Shared;

public sealed class RequestLoggerTests
{
    private readonly ILogger<CreateSurveyCommand> _logger = Substitute.For<ILogger<CreateSurveyCommand>>();
    private readonly IUserService _userService = Substitute.For<IUserService>();

    [Fact]
    public async Task ShouldCallGetUserIdentityOnce()
    {
        var requestLogger = new LoggingBehaviour<CreateSurveyCommand>(_logger, _userService);

        await requestLogger.Process(new CreateSurveyCommand("Test Topic", 10, "Test Respondents",
            new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = "Option 1"
                }
            }), CancellationToken.None);

        _userService.Received(1).GetUserIdentity();
    }
}