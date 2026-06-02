using AutoFixture;
using FakeSurveyGenerator.Application.Features.Users;
using FakeSurveyGenerator.Application.Shared.Identity;
using FakeSurveyGenerator.Application.TestHelpers;
using FakeSurveyGenerator.Application.Tests.Setup;
using NSubstitute;
using RegisterUserCommandHandler = FakeSurveyGenerator.Application.Features.Users.RegisterUserCommandHandler;

namespace FakeSurveyGenerator.Application.Tests.Features.Users;

public sealed class RegisterUserCommandTests
{
    [ClassDataSource<TestFixture>]
    public required TestFixture Fixture { get; init; }

    private readonly IFixture _fixture = new Fixture();
    private readonly IUserService _mockUserService = Substitute.For<IUserService>();

    public RegisterUserCommandTests()
    {
        _mockUserService.GetUserInfo(Arg.Any<CancellationToken>()).Returns(TestUser.Instance);
    }

    [Test]
    public async Task GivenValidRegisterUserCommand_WhenCallingHandle_ThenResultShouldBeSuccessful()
    {
        var registerUserCommand = new RegisterUserCommand();

        var mockUserService = Substitute.For<IUserService>();
        mockUserService.GetUserInfo(Arg.Any<CancellationToken>()).Returns(new TestUser(_fixture.Create<string>(),
            _fixture.Create<string>(), _fixture.Create<string>()));

        var sut = new RegisterUserCommandHandler(mockUserService, Fixture.Context);

        var result = await sut.Handle(registerUserCommand, CancellationToken.None);

        await Assert.That(result.IsNewRegistration).IsTrue();
    }

    [Test]
    [MethodDataSource(typeof(RegisterUserCommandTestDataSources), nameof(RegisterUserCommandTestDataSources.RegisterUserCommandData))]
    public async Task GivenValidRegisterUserCommand_WhenCallingHandle_ThenNewUserShouldBeReturned(string newUserId,
        string newUserDisplayName, string newUserEmailAddress)
    {
        var registerUserCommand = new RegisterUserCommand();

        var mockUserService = Substitute.For<IUserService>();
        mockUserService.GetUserInfo(Arg.Any<CancellationToken>())
            .Returns(new TestUser(newUserId, newUserDisplayName, newUserEmailAddress));

        var sut = new RegisterUserCommandHandler(mockUserService, Fixture.Context);

        var result = await sut.Handle(registerUserCommand, CancellationToken.None);

        await Assert.That(result.User.Id).IsPositive();
        await Assert.That(result.User.ExternalUserId).IsEqualTo(newUserId);
        await Assert.That(result.User.DisplayName).IsEqualTo(newUserDisplayName);
        await Assert.That(result.User.EmailAddress).IsEqualTo(newUserEmailAddress);
        await Assert.That(result.IsNewRegistration).IsTrue();
    }

    [Test]
    public async Task GivenAlreadyExistingUser_WhenCallingHandle_ThenExistingUserShouldBeReturned()
    {
        var registerUserCommand = new RegisterUserCommand();

        var sut = new RegisterUserCommandHandler(_mockUserService, Fixture.Context);

        var result = await sut.Handle(registerUserCommand, CancellationToken.None);

        await Assert.That(result.IsNewRegistration).IsFalse();
        await Assert.That(result.User.ExternalUserId).IsEqualTo(TestUser.Instance.Id);
    }

    public static class RegisterUserCommandTestDataSources
    {
        public static Func<(string, string, string)> RegisterUserCommandData()
        {
            var fixture = new Fixture();
            return () => (fixture.Create<string>(), fixture.Create<string>(), fixture.Create<string>());
        }
    }
}