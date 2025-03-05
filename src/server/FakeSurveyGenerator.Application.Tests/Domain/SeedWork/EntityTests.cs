using AutoFixture;
using FakeSurveyGenerator.Application.Domain.Shared.SeedWork;
using TUnit.Assertions.AssertConditions.Throws;

namespace FakeSurveyGenerator.Application.Tests.Domain.SeedWork;

public sealed class EntityTests
{
    private readonly Fixture _fixture = new();

    #region Test Entities

    // Simple concrete implementations for testing
    private class TestEntityInt : Entity
    {
        public TestEntityInt() { }

        public TestEntityInt(int id)
        {
            Id = id;
        }
    }

    private class TestEntityString : Entity<string>
    {
        public TestEntityString() { }

        public TestEntityString(string id)
        {
            Id = id;
        }
    }

    private class TestDomainEvent(string eventData) : DomainEvent
    {
        public string EventData { get; } = eventData;
    }

    private class DerivedTestEntityInt(int id) : TestEntityInt(id);

    #endregion

    #region Domain Events Tests

    [Test]
    public async Task GivenNewEntity_WhenInitialized_ThenDomainEventsCollectionIsEmpty()
    {
        var entity = new TestEntityInt();

        await Assert.That(entity.DomainEvents).IsNotNull();
        await Assert.That(entity.DomainEvents.Count).IsEqualTo(0);
    }

    [Test]
    public async Task GivenEntity_WhenDomainEventAdded_ThenEventIsInDomainEventsCollection()
    {
        // Arrange
        var entity = new TestEntityInt();
        var domainEvent = new TestDomainEvent(_fixture.Create<string>());

        // Act
        entity.AddDomainEvent(domainEvent);

        // Assert
        await Assert.That(entity.DomainEvents.Count).IsEqualTo(1);
        await Assert.That(entity.DomainEvents.First()).IsEqualTo(domainEvent);
    }

    [Test]
    public async Task GivenEntityWithDomainEvents_WhenEventRemoved_ThenEventIsNotInCollection()
    {
        // Arrange
        var entity = new TestEntityInt();
        var domainEvent1 = new TestDomainEvent(_fixture.Create<string>());
        var domainEvent2 = new TestDomainEvent(_fixture.Create<string>());
        entity.AddDomainEvent(domainEvent1);
        entity.AddDomainEvent(domainEvent2);

        // Act
        entity.RemoveDomainEvent(domainEvent1);

        // Assert
        await Assert.That(entity.DomainEvents.Count).IsEqualTo(1);
        await Assert.That(entity.DomainEvents.Contains(domainEvent1)).IsFalse();
        await Assert.That(entity.DomainEvents.Contains(domainEvent2)).IsTrue();
    }

    [Test]
    public async Task GivenEntityWithDomainEvents_WhenClearingEvents_ThenAllEventsAreRemoved()
    {
        // Arrange
        var entity = new TestEntityInt();
        entity.AddDomainEvent(new TestDomainEvent(_fixture.Create<string>()));
        entity.AddDomainEvent(new TestDomainEvent(_fixture.Create<string>()));

        // Act
        entity.ClearDomainEvents();

        // Assert
        await Assert.That(entity.DomainEvents.Count).IsEqualTo(0);
    }

    [Test]
    public async Task GivenNullDomainEvent_WhenAddingEvent_ThenArgumentNullExceptionIsThrown()
    {
        // Arrange
        var entity = new TestEntityInt();

        // Act & Assert
        await Assert.That(() => entity.AddDomainEvent(null!)).ThrowsException().And.IsTypeOf<ArgumentNullException>();
    }

    [Test]
    public async Task GivenNullDomainEvent_WhenRemovingEvent_ThenArgumentNullExceptionIsThrown()
    {
        // Arrange
        var entity = new TestEntityInt();

        // Act & Assert
        await Assert.That(() => entity.RemoveDomainEvent(null!)).ThrowsException().And.IsTypeOf<ArgumentNullException>();
    }

    #endregion

    #region Equality Tests - Generic Entity<TId>

    [Test]
    public async Task GivenTwoGenericEntitiesWithSameId_WhenComparingEquality_ThenEntitiesAreEqual()
    {
        // Arrange
        var id = _fixture.Create<string>();
        var entity1 = new TestEntityString(id);
        var entity2 = new TestEntityString(id);

        // Assert
        await Assert.That(entity1.Equals(entity2)).IsTrue();
        await Assert.That(entity1 == entity2).IsTrue();
        await Assert.That(entity1 != entity2).IsFalse();
    }

    [Test]
    public async Task GivenTwoGenericEntitiesWithDifferentIds_WhenComparingEquality_ThenEntitiesAreNotEqual()
    {
        // Arrange
        var entity1 = new TestEntityString(_fixture.Create<string>());
        var entity2 = new TestEntityString(_fixture.Create<string>());

        // Assert
        await Assert.That(entity1.Equals(entity2)).IsFalse();
        await Assert.That(entity1 == entity2).IsFalse();
        await Assert.That(entity1 != entity2).IsTrue();
    }

    [Test]
    public async Task GivenGenericEntityComparedWithNull_WhenComparingEquality_ThenEntitiesAreNotEqual()
    {
        // Arrange
        var entity = new TestEntityString(_fixture.Create<string>());

        // Assert
        await Assert.That(entity.Equals(null)).IsFalse();
        await Assert.That(entity == null).IsFalse();
        await Assert.That(entity != null).IsTrue();
        await Assert.That(null == entity).IsFalse();
        await Assert.That(null != entity).IsTrue();
    }

    [Test]
    public async Task GivenTwoNullGenericEntities_WhenComparingEquality_ThenEntitiesAreEqual()
    {
        // Assert
        TestEntityString? entity1 = null;
        TestEntityString? entity2 = null;

        await Assert.That(entity1 == entity2).IsTrue();
        await Assert.That(entity1 != entity2).IsFalse();
    }

    [Test]
    public async Task GivenTwoGenericEntitiesOfDifferentTypes_WhenComparingEquality_ThenEntitiesAreNotEqual()
    {
        // Arrange
        var entity1 = new TestEntityString("1");
        var entity2 = new TestEntityInt(1);

        // Assert
        // ReSharper disable once SuspiciousTypeConversion.Global
        await Assert.That(entity1.Equals(entity2)).IsFalse();
    }

    [Test]
    public async Task GivenTwoGenericTransientEntities_WhenComparingEquality_ThenEntitiesAreNotEqual()
    {
        // Arrange
        var entity1 = new TestEntityString();
        var entity2 = new TestEntityString();

        // Assert
        await Assert.That(entity1.Equals(entity2)).IsFalse();
    }

    #endregion

    #region Equality Tests - Non-Generic Entity

    [Test]
    public async Task GivenTwoEntitiesWithSameId_WhenComparingEquality_ThenEntitiesAreEqual()
    {
        // Arrange
        var id = _fixture.Create<int>();
        var entity1 = new TestEntityInt(id);
        var entity2 = new TestEntityInt(id);

        // Assert
        await Assert.That(entity1.Equals(entity2)).IsTrue();
        await Assert.That(entity1 == entity2).IsTrue();
        await Assert.That(entity1 != entity2).IsFalse();
    }

    [Test]
    public async Task GivenTwoEntitiesWithDifferentIds_WhenComparingEquality_ThenEntitiesAreNotEqual()
    {
        // Arrange
        var entity1 = new TestEntityInt(_fixture.Create<int>());
        var entity2 = new TestEntityInt(_fixture.Create<int>());

        // Assert
        await Assert.That(entity1.Equals(entity2)).IsFalse();
        await Assert.That(entity1 == entity2).IsFalse();
        await Assert.That(entity1 != entity2).IsTrue();
    }

    [Test]
    public async Task GivenEntityComparedWithNull_WhenComparingEquality_ThenEntitiesAreNotEqual()
    {
        // Arrange
        var entity = new TestEntityInt(_fixture.Create<int>());

        // Assert
        await Assert.That(entity.Equals(null)).IsFalse();
        await Assert.That(entity == null).IsFalse();
        await Assert.That(entity != null).IsTrue();
        await Assert.That(null == entity).IsFalse();
        await Assert.That(null != entity).IsTrue();
    }

    [Test]
    public async Task GivenTwoNullEntities_WhenComparingEquality_ThenEntitiesAreEqual()
    {
        // Assert
        TestEntityInt? entity1 = null;
        TestEntityInt? entity2 = null;

        await Assert.That(entity1 == entity2).IsTrue();
        await Assert.That(entity1 != entity2).IsFalse();
    }

    [Test]
    public async Task GivenTwoTransientEntities_WhenComparingEquality_ThenEntitiesAreNotEqual()
    {
        // Arrange
        var entity1 = new TestEntityInt();
        var entity2 = new TestEntityInt();

        // Assert
        await Assert.That(entity1.Equals(entity2)).IsFalse();
    }

    [Test]
    public async Task GivenEntityWithIdAndTransientEntity_WhenComparingEquality_ThenEntitiesAreNotEqual()
    {
        // Arrange
        var entity1 = new TestEntityInt(_fixture.Create<int>());
        var entity2 = new TestEntityInt();

        // Assert
        await Assert.That(entity1.Equals(entity2)).IsFalse();
    }

    [Test]
    public async Task GivenEntityAndItsSubclass_WhenComparingEquality_ThenEntitiesAreNotEqual()
    {
        // Arrange
        var id = _fixture.Create<int>();
        var entity1 = new TestEntityInt(id);
        var entity2 = new DerivedTestEntityInt(id);

        // Assert
        await Assert.That(entity1.Equals(entity2)).IsFalse();
    }

    #endregion

    #region GetHashCode Tests

    [Test]
    public async Task GivenTwoEntitiesWithSameId_WhenGettingHashCode_ThenHashCodesAreEqual()
    {
        // Arrange
        var id = _fixture.Create<int>();
        var entity1 = new TestEntityInt(id);
        var entity2 = new TestEntityInt(id);

        // Assert
        await Assert.That(entity1.GetHashCode()).IsEqualTo(entity2.GetHashCode());
    }

    [Test]
    public async Task GivenTwoEntitiesWithDifferentIds_WhenGettingHashCode_ThenHashCodesAreDifferent()
    {
        // Arrange
        var entity1 = new TestEntityInt(_fixture.Create<int>());
        var entity2 = new TestEntityInt(_fixture.Create<int>());

        // Assert
        await Assert.That(entity1.GetHashCode()).IsNotEqualTo(entity2.GetHashCode());
    }

    [Test]
    public async Task GivenTransientEntity_WhenGettingHashCode_ThenHashCodeIsUnique()
    {
        // Arrange
        var entity1 = new TestEntityInt();
        var entity2 = new TestEntityInt();

        // Assert
        await Assert.That(entity1.GetHashCode()).IsNotEqualTo(entity2.GetHashCode());
    }

    #endregion
}

