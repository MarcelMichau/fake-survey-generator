using FakeSurveyGenerator.Application.Features.Surveys;
using FakeSurveyGenerator.Application.Shared.Errors;
using FakeSurveyGenerator.Application.Shared.Identity;
using FakeSurveyGenerator.Application.TestHelpers;
using FakeSurveyGenerator.Application.Tests.Setup;
using FluentValidation.TestHelper;
using NSubstitute;

namespace FakeSurveyGenerator.Application.Tests.Features.Surveys;

public sealed class SurveyInputEdgeCaseTests
{
    private static readonly TestUser UnregisteredUser = new("unregistered-id", "Unregistered", "unregistered@test.com");

    [Test]
    public async Task GivenNullSurveyOption_WhenValidatingCreateSurveyCommand_ThenIsValidShouldBeFalse()
    {
        var command = new CreateSurveyCommand
        {
            SurveyTopic = "Topic",
            NumberOfRespondents = 1,
            RespondentType = "Testers",
            SurveyOptions = [null!]
        };

        var result = new CreateSurveyCommandValidator().TestValidate(command);

        await Assert.That(result.IsValid).IsFalse();
    }

    [Test]
    public async Task GivenNullSurveyOption_WhenValidatingAnalyzeSurveyCommand_ThenIsValidShouldBeFalse()
    {
        var command = new AnalyzeSurveyCommand
        {
            SurveyTopic = "Topic",
            RespondentType = "Testers",
            SurveyOptions = [new SurveyOptionDto { OptionText = "Valid" }, null!]
        };

        var result = new AnalyzeSurveyCommandValidator().TestValidate(command);

        await Assert.That(result.IsValid).IsFalse();
    }

    [Test]
    public async Task GivenNullSurveyOption_WhenCallingCreateSurveyHandle_ThenValidationErrorShouldBeReturnedWithoutThrowing()
    {
        using var context = SurveyContextFactory.Create();
        await SurveyContextFactory.SeedSampleData(context);
        var sut = new CreateSurveyCommandHandler(context, UserService(TestUser.Instance),
            new CreateSurveyCommandValidator());

        var result = await sut.Handle(new CreateSurveyCommand
        {
            SurveyTopic = "Topic",
            NumberOfRespondents = 1,
            RespondentType = "Testers",
            SurveyOptions = [null!]
        });

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsTypeOf<ValidationError>();
    }

    [Test]
    public async Task GivenUnregisteredUser_WhenCallingCreateSurveyHandle_ThenUserNotRegisteredErrorShouldBeReturned()
    {
        using var context = SurveyContextFactory.Create();
        var sut = new CreateSurveyCommandHandler(context, UserService(UnregisteredUser),
            new CreateSurveyCommandValidator());

        var result = await sut.Handle(new CreateSurveyCommand
        {
            SurveyTopic = "Topic",
            NumberOfRespondents = 5,
            RespondentType = "Testers",
            SurveyOptions = [new SurveyOptionDto { OptionText = "Option" }]
        });

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(Errors.General.UserNotRegistered());
    }

    [Test]
    public async Task GivenUnregisteredUser_WhenCallingDeleteSurveyHandle_ThenUserNotRegisteredErrorShouldBeReturned()
    {
        using var context = SurveyContextFactory.Create();
        await SurveyContextFactory.SeedSampleData(context);
        var sut = new DeleteSurveyCommandHandler(context, UserService(UnregisteredUser), new TestHybridCache(),
            new DeleteSurveyCommandValidator());

        var result = await sut.Handle(new DeleteSurveyCommand(1));

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(Errors.General.UserNotRegistered());
        await Assert.That(context.Surveys.Any()).IsTrue();
    }

    private static IUserService UserService(IUser user)
    {
        var userService = Substitute.For<IUserService>();
        userService.GetUserInfo(Arg.Any<CancellationToken>()).Returns(user);
        userService.GetUserIdentity().Returns(user.Id);
        return userService;
    }
}
