using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Common.Errors;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Application.Users.Commands.RegisterUser;
using FakeSurveyGenerator.Data;
using Moq;
using Shouldly;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests.Users.Commands.RegisterUser
{
    public sealed class RegisterUserCommandTests : CommandTestBase
    {
        [Fact]
        public async Task GivenNewUser_Handle_ShouldBeAbleToRegisterUser()
        {
            var registerUserCommand = new RegisterUserCommand();

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(service => service.GetUserInfo(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TestUser("new-id", "New User", "new.user@test.com"));

            var sut = new RegisterUserCommandHandler(mockUserService.Object, Context, Mapper);

            var result = await sut.Handle(registerUserCommand, CancellationToken.None);

            result.IsSuccess.ShouldBe(true);
        }

        [Fact]
        public async Task GivenNewUser_Handle_ShouldReturnNewRegisteredUser()
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

            result.Value.Id.ShouldBeGreaterThan(0);
            result.Value.DisplayName.ShouldBe(expectedNewUserDisplayName);
            result.Value.EmailAddress.ShouldBe(expectedNewUserEmailAddress);
            result.Value.ExternalUserId.ShouldBe(expectedNewUserId);
        }

        [Fact]
        public async Task GivenAlreadyExistingUser_Handle_ShouldNotRegisterUser()
        {
            var registerUserCommand = new RegisterUserCommand();

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(service => service.GetUserInfo(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TestUser("test-id", "Test User", "test.user@test.com"));

            var sut = new RegisterUserCommandHandler(mockUserService.Object, Context, Mapper);

            var result = await sut.Handle(registerUserCommand, CancellationToken.None);

            result.IsFailure.ShouldBe(true);
        }

        [Fact]
        public async Task GivenAlreadyExistingUser_Handle_ShouldReturnUserAlreadyRegisteredError()
        {
            var registerUserCommand = new RegisterUserCommand();

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(service => service.GetUserInfo(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TestUser("test-id", "Test User", "test.user@test.com"));

            var sut = new RegisterUserCommandHandler(mockUserService.Object, Context, Mapper);

            var result = await sut.Handle(registerUserCommand, CancellationToken.None);

            result.Error.Code.ShouldBe(Errors.General.UserAlreadyRegistered().Code);
        }
    }
}
