using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Common.Behaviours;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests.Common.Behaviours
{
    public sealed class RequestLoggerTests
    {
        private readonly ILogger<CreateSurveyCommand> _logger;
        private readonly IUserService _userService;

        public RequestLoggerTests()
        {
            _logger = Substitute.For<ILogger<CreateSurveyCommand>>();
            _userService = Substitute.For<IUserService>();
        }

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
}
