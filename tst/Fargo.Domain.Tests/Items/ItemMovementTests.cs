using Fargo.Core.Articles;
using Fargo.Core.Events;
using Fargo.Core.Items;

namespace Fargo.Core.Tests.Items;

public sealed class ItemMovementTests
{
    [Fact]
    public void Moved_Should_SetMovementDetailsAndEventProperties()
    {
        var item = Item.CreateItem(Article.CreateArticle(new Name("Article"), CreateDomainActor()));
        var fromParentGuid = Guid.NewGuid();
        var toParentGuid = Guid.NewGuid();
        var actorGuid = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        var movement = ItemMovement.Moved(
            item,
            fromParentGuid,
            toParentGuid,
            actorGuid,
            occurredAt);

        Assert.Equal(movement.Event.Guid, movement.Guid);
        Assert.Equal(EntityType.Item, movement.Event.EntityType);
        Assert.Equal(EntityEventType.Moved, movement.Event.EventType);
        Assert.Equal(item.Guid, movement.Event.EntityGuid);
        Assert.Equal(item.Guid, movement.ItemGuid);
        Assert.Equal(fromParentGuid, movement.FromParentContainerGuid);
        Assert.Equal(toParentGuid, movement.ToParentContainerGuid);
        Assert.Equal(actorGuid, movement.ActorGuid);
        Assert.Equal(occurredAt, movement.OccurredAt);
    }

    [Fact]
    public void Moved_Should_Throw_WhenRequiredArgumentsAreInvalid()
    {
        var item = Item.CreateItem(Article.CreateArticle(new Name("Article"), CreateDomainActor()));
        var occurredAt = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentNullException>(() => ItemMovement.Moved(
            null!,
            null,
            null,
            Guid.NewGuid(),
            occurredAt));

        Assert.Throws<ArgumentException>(() => ItemMovement.Moved(
            item,
            null,
            null,
            Guid.Empty,
            occurredAt));
    }
}
