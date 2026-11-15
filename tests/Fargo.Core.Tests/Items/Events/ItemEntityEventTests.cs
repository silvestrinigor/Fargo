using Fargo.Core.Common;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Events;
using Fargo.Core.Informations;

namespace Fargo.Core.Tests.Items.Events;

public class ItemEntityEventTests
{
    private static Item Widget() => Item.CreateItem(Article.NewArticle(new Name("Widget")));

    private static Item Container() => Item.CreateItem(Article.NewArticleContainer(new Name("Box")));

    [Fact]
    public void NewItemCreatedEvent_WhenInsideContainer_ShouldRecordTheContainerAndTheItem()
    {
        var item = Widget();
        var container = Container();
        item.PlaceInsideContainer(container);

        var itemEvent = ItemEvent.NewItemCreatedEvent(item);

        Assert.Equal(ItemEventType.Creation, itemEvent.ItemEventType);
        Assert.Equal(item.Guid, itemEvent.ItemGuid);
        Assert.Equal(item.ArticleGuid, itemEvent.ArticleGuid);
        Assert.Equal(1, itemEvent.Quantity.Amount);
        Assert.True(itemEvent.FromNoContainer);
        Assert.Equal(container.Guid, itemEvent.ToItemContainerGuid);
        Assert.False(itemEvent.ToNoContainer);
    }

    [Fact]
    public void NewItemCreatedEvent_WhenNotInAContainer_ShouldFlagNoDestination()
    {
        var itemEvent = ItemEvent.NewItemCreatedEvent(Widget());

        Assert.Null(itemEvent.ToItemContainerGuid);
        Assert.True(itemEvent.ToNoContainer);
    }

    [Fact]
    public void NewItemMovedEvent_ShouldRecordThePreviousAndTheCurrentContainer()
    {
        var item = Widget();
        var first = Container();
        var second = Container();
        item.PlaceInsideContainer(first);
        item.PlaceInsideContainer(second);

        var itemEvent = ItemEvent.NewItemMovedEvent(item, first.Guid);

        Assert.Equal(ItemEventType.Moviment, itemEvent.ItemEventType);
        Assert.Equal(first.Guid, itemEvent.FromItemContainerGuid);
        Assert.False(itemEvent.FromNoContainer);
        Assert.Equal(second.Guid, itemEvent.ToItemContainerGuid);
    }

    [Fact]
    public void NewItemMovedEvent_WhenRemovedFromAllContainers_ShouldFlagNoDestination()
    {
        var item = Widget();
        var container = Container();
        item.PlaceInsideContainer(container);
        item.RemoveFromContainers();

        var itemEvent = ItemEvent.NewItemMovedEvent(item, container.Guid);

        Assert.Equal(container.Guid, itemEvent.FromItemContainerGuid);
        Assert.Null(itemEvent.ToItemContainerGuid);
        Assert.True(itemEvent.ToNoContainer);
    }

    [Fact]
    public void NewItemDeletedEvent_ShouldRecordTheContainerTheItemWasIn()
    {
        var item = Widget();
        var container = Container();
        item.PlaceInsideContainer(container);

        var itemEvent = ItemEvent.NewItemDeletedEvent(item);

        Assert.Equal(ItemEventType.Deletion, itemEvent.ItemEventType);
        Assert.Equal(container.Guid, itemEvent.FromItemContainerGuid);
        Assert.True(itemEvent.ToNoContainer);
    }

    [Fact]
    public void NewContainedItemDeletedEvent_ShouldKeepOnlyTheFlagOfTheDeletedContainer()
    {
        var itemGuid = Guid.NewGuid();
        var articleGuid = Guid.NewGuid();

        var itemEvent = ItemEvent.NewContainedItemDeletedEvent(itemGuid, articleGuid);

        Assert.Equal(itemGuid, itemEvent.ItemGuid);
        Assert.Equal(articleGuid, itemEvent.ArticleGuid);
        Assert.Null(itemEvent.FromItemContainerGuid);
        Assert.False(itemEvent.FromNoContainer);
    }
}
