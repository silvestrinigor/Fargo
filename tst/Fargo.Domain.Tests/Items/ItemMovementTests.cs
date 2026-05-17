using Fargo.Core.Items;

namespace Fargo.Core.Tests.Items;

public sealed class ItemMovementTests
{
    [Fact]
    public void Constructor_Should_SetProperties()
    {
        var itemGuid = Guid.NewGuid();
        var fromParentGuid = Guid.NewGuid();
        var toParentGuid = Guid.NewGuid();
        var actorGuid = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        var movement = new ItemMovement(
            itemGuid,
            fromParentGuid,
            toParentGuid,
            actorGuid,
            occurredAt);

        Assert.Equal(itemGuid, movement.ItemGuid);
        Assert.Equal(fromParentGuid, movement.FromParentContainerGuid);
        Assert.Equal(toParentGuid, movement.ToParentContainerGuid);
        Assert.Equal(actorGuid, movement.ActorGuid);
        Assert.Equal(occurredAt, movement.OccurredAt);
    }

    [Fact]
    public void Constructor_Should_Throw_WhenRequiredGuidsAreEmpty()
    {
        var validGuid = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(() => new ItemMovement(
            Guid.Empty,
            null,
            null,
            validGuid,
            occurredAt));
        Assert.Throws<ArgumentException>(() => new ItemMovement(
            validGuid,
            null,
            null,
            Guid.Empty,
            occurredAt));
    }
}
