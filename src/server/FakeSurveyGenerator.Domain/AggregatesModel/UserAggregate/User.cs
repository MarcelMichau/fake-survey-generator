using System.Collections.Generic;
using System.Linq;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.Common;
using FakeSurveyGenerator.Shared.SeedWork;
using JetBrains.Annotations;

namespace FakeSurveyGenerator.Domain.AggregatesModel.UserAggregate;

public sealed class User : AuditableEntity, IAggregateRoot
{
    public NonEmptyString DisplayName { get; }
    public NonEmptyString EmailAddress { get; }
    public NonEmptyString ExternalUserId { get; }

    private readonly List<Survey> _ownedSurveys = new();
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