using FakeSurveyGenerator.Application.Domain.Shared;
using FakeSurveyGenerator.Application.Domain.Shared.SeedWork;
using FakeSurveyGenerator.Application.Domain.Surveys;
using JetBrains.Annotations;

namespace FakeSurveyGenerator.Application.Domain.Users;

public sealed class User : AuditableEntity, IAggregateRoot
{
    private readonly List<Survey> _ownedSurveys = [];

    [UsedImplicitly]
    private User()
    {
    } // Necessary for Entity Framework Core

    public User(NonEmptyString displayName, NonEmptyString emailAddress, NonEmptyString externalUserId)
    {
        DisplayName = displayName;
        EmailAddress = emailAddress;
        ExternalUserId = externalUserId;
    }

    public NonEmptyString DisplayName { get; } = null!;
    public NonEmptyString EmailAddress { get; } = null!;
    public NonEmptyString ExternalUserId { get; } = null!;
    public IReadOnlyList<Survey> OwnedSurveys => _ownedSurveys.ToList();
}