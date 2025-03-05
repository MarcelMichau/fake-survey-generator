using System.Runtime.CompilerServices;

namespace FakeSurveyGenerator.Application.Domain.Shared.SeedWork;

/// <summary>
/// Base class for all domain entities with a strongly-typed identifier
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier</typeparam>
public abstract class Entity<TId> : IHasDomainEvents, IEquatable<Entity<TId>>
{
    private readonly Lazy<int> _requestedHashCode;
    private readonly List<DomainEvent> _domainEvents = [];

    /// <summary>
    /// The unique identifier of the entity
    /// </summary>
    public virtual TId Id { get; protected set; } = default!;

    /// <summary>
    /// Domain events raised by this entity
    /// </summary>
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected Entity()
    {
        _requestedHashCode = new Lazy<int>(() =>
                Id is null ? 31 : HashCode.Combine(Id),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>
    /// Adds a domain event to this entity
    /// </summary>
    /// <param name="domainEvent">The domain event to add</param>
    public void AddDomainEvent(DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a domain event from this entity
    /// </summary>
    /// <param name="domainEvent">The domain event to remove</param>
    public void RemoveDomainEvent(DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clears all domain events from this entity
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Determines if this entity is transient (not yet persisted to the database)
    /// </summary>
    /// <returns>True if the entity is transient, otherwise false</returns>
    protected virtual bool IsTransient()
    {
        return Id is null || Id.Equals(default(TId));
    }

    /// <summary>
    /// Determines whether this entity is equal to another object
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is Entity<TId> entity)
        {
            return Equals(entity);
        }

        return false;
    }

    /// <summary>
    /// Determines whether this entity is equal to another entity of the same type
    /// </summary>
    public virtual bool Equals(Entity<TId>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (IsTransient() || other.IsTransient())
            return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <summary>
    /// Gets a hash code for this entity
    /// </summary>
    public override int GetHashCode()
    {
        // For transient entities, use a combination of type hash and system identity hash
        // instead of relying on base.GetHashCode()
        return IsTransient() ? HashCode.Combine(GetType().Name, RuntimeHelpers.GetHashCode(this)) : _requestedHashCode.Value;
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            return true;

        if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }
}
