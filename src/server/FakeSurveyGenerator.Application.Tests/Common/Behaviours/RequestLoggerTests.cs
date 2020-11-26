using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Common.Behaviours;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests.Common.Behaviours
{
    public sealed class RequestLoggerTests
    {
        private readonly Mock<ILogger<CreateSurveyCommand>> _logger;
        private readonly Mock<IUserService> _userService;

        public RequestLoggerTests()
        {
            _logger = new Mock<ILogger<CreateSurveyCommand>>();
            _userService = new Mock<IUserService>();
        }

        [Fact]
        public async Task ShouldCallGetUserIdentityOnce()
        {
            var requestLogger = new LoggingBehaviour<CreateSurveyCommand>(_logger.Object, _userService.Object);

            await requestLogger.Process(new CreateSurveyCommand("Test Topic", 10, "Test Respondents",
                new List<SurveyOptionDto>
                {
                    new()
                    {
                        OptionText = "Option 1"
                    }
                }), CancellationToken.None);

            _userService.Verify(i => i.GetUserIdentity(), Times.Once);
        }
    }
}
