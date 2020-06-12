using System.Collections.Generic;
using System.Linq;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.Common;
using FakeSurveyGenerator.Domain.SeedWork;

namespace FakeSurveyGenerator.Domain.AggregatesModel.UserAggregate
{
    public class User: Entity, IAggregateRoot
    {
        public NonEmptyString DisplayName { get; }
        public NonEmptyString EmailAddress { get; }
        public NonEmptyString ExternalUserId { get; }

        private readonly List<Survey> _ownedSurveys = new List<Survey>();
        public IReadOnlyList<Survey> OwnedSurveys => _ownedSurveys.ToList();

        public User(NonEmptyString displayName, NonEmptyString emailAddress, NonEmptyString externalUserId)
        {
            DisplayName = displayName;
            EmailAddress = emailAddress;
            ExternalUserId = externalUserId;
        }
    }
}
