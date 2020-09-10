using System;
using System.Collections.Generic;

namespace FakeSurveyGenerator.Shared.SeedWork
{
    public interface IHasDomainEvents
    {
        public IReadOnlyCollection<DomainEvent> DomainEvents { get; }
    }

    public abstract class DomainEvent
    {
        protected DomainEvent()
        {
            DateOccurred = DateTimeOffset.UtcNow;
        }

        public DateTimeOffset DateOccurred { get; protected set; }
    }
}
