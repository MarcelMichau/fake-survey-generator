using FakeSurveyGenerator.Application.Domain.Shared;
using FakeSurveyGenerator.Application.Domain.Shared.SeedWork;
using FakeSurveyGenerator.Application.Domain.Surveys;
using JetBrains.Annotations;

namespace FakeSurveyGenerator.Application.Domain.Users;

public sealed class User : AuditableEntity, IAggregateRoot
{
    public NonEmptyString DisplayName { get; } = null!;
    public NonEmptyString EmailAddress { get; } = null!;
    public NonEmptyString ExternalUserId { get; } = null!;

    private readonly List<Survey> _ownedSurveys = [];
    public IReadOnlyList<Survey> OwnedSurveys => _ownedSurveys.ToList();

    [UsedImplicitly]
    private User() { } // Necessary for Entity Framework Core + AutoMapper

    public User(NonEmptyString displayName, NonEmptyString emailAddress, NonEmptyString externalUserId)
    {
        DisplayName = displayName;
        EmailAddress = emailAddress;
        ExternalUserId = externalUserId;
    }
}