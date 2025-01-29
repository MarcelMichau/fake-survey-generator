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

    [Test]
    public async Task ShouldCallGetUserIdentityOnce()
    {
        var requestLogger = new LoggingBehaviour<CreateSurveyCommand>(_logger, _userService);

        var createSurveyCommand = new CreateSurveyCommand
        {
            SurveyTopic = "Test Topic",
            NumberOfRespondents = 10,
            RespondentType = "Test Respondents",
            SurveyOptions = new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = "Option 1"
                }
            }
        };

        await requestLogger.Process(createSurveyCommand, CancellationToken.None);

        _userService.Received(1).GetUserIdentity();
    }
}
