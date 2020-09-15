using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Common.Errors;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Application.Users.Commands.RegisterUser;
using FakeSurveyGenerator.Data;
using FluentAssertions;
using Moq;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests.Users.Commands.RegisterUser
{
    public sealed class RegisterUserCommandTests : CommandTestBase
    {
        [Fact]
        public async Task GivenValidRegisterUserCommand_WhenCallingHandle_ThenResultShouldBeSuccessful()
        {
            var registerUserCommand = new RegisterUserCommand();

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(service => service.GetUserInfo(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TestUser("new-id", "New User", "new.user@test.com"));

            var sut = new RegisterUserCommandHandler(mockUserService.Object, Context, Mapper);

            var result = await sut.Handle(registerUserCommand, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task GivenValidRegisterUserCommand_WhenCallingHandle_ThenNewUserShouldBeReturned()
        {
            var registerUserCommand = new RegisterUserCommand();

            const string expectedNewUserId = "new-id";
            const string expectedNewUserDisplayName = "New User";
            const string expectedNewUserEmailAddress = "new.user@test.com";

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(service => service.GetUserInfo(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TestUser(expectedNewUserId, expectedNewUserDisplayName, expectedNewUserEmailAddress));

            var sut = new RegisterUserCommandHandler(mockUserService.Object, Context, Mapper);

            var result = await sut.Handle(registerUserCommand, CancellationToken.None);

            result.Value.Id.Should().BePositive();
            result.Value.DisplayName.Should().Be(expectedNewUserDisplayName);
            result.Value.EmailAddress.Should().Be(expectedNewUserEmailAddress);
            result.Value.ExternalUserId.Should().Be(expectedNewUserId);
        }

        [Fact]
        public async Task GivenAlreadyExistingUser_WhenCallingHandle_ThenResultShouldBeFailure()
        {
            var registerUserCommand = new RegisterUserCommand();

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(service => service.GetUserInfo(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TestUser("test-id", "Test User", "test.user@test.com"));

            var sut = new RegisterUserCommandHandler(mockUserService.Object, Context, Mapper);

            var result = await sut.Handle(registerUserCommand, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
        }

        [Fact]
        public async Task GivenAlreadyExistingUser_WhenCallingHandle_ThenUserAlreadyRegisteredErrorShouldBeReturned()
        {
            var registerUserCommand = new RegisterUserCommand();

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(service => service.GetUserInfo(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TestUser("test-id", "Test User", "test.user@test.com"));

            var sut = new RegisterUserCommandHandler(mockUserService.Object, Context, Mapper);

            var result = await sut.Handle(registerUserCommand, CancellationToken.None);

            result.Error.Code.Should().Be(Errors.General.UserAlreadyRegistered().Code);
        }
    }
}
