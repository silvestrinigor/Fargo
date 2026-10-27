using Fargo.Core.Articles;
using Fargo.Core.Common;
using Fargo.Core.Informations;
using Fargo.Core.Items;
using Fargo.Core.Items.Events;
using UnitsNet;

namespace Fargo.Core.Tests.Items.Events;

public class ItemLifecycleEventTests
{
    private static Article EventArticle() =>
        Article.NewArticle(new Name("Bulk sand"), ArticleTrackingMode.Event);

    private static Article EntityArticle() =>
        Article.NewArticle(new Name("Widget"), ArticleTrackingMode.Entity);

    private static Item Container() =>
        Item.CreateItem(Article.NewArticleContainer(new Name("Silo")));

    private static Scalar Qty(double amount) => Scalar.FromAmount(amount);

    [Fact]
    public void NewCreationEvent_WhenNoContainer_ShouldRecordQuantity()
    {
        var article = EventArticle();

        var itemEvent = ItemEvent.NewCreationEvent(article, null, Qty(100));

        Assert.Equal(article.Guid, itemEvent.ArticleGuid);
        Assert.Equal(ItemEventType.Creation, itemEvent.EventType);
        Assert.Null(itemEvent.Moviment);

        var lifecycle = itemEvent.Lifecycle;
        Assert.NotNull(lifecycle);
        Assert.Equal(itemEvent.Guid, lifecycle!.ItemEventGuid);
        Assert.Equal(100, lifecycle.Quantity.Amount);
        Assert.Null(lifecycle.ItemContainerGuid);
        Assert.True(lifecycle.NoContainer);
    }

    [Fact]
    public void NewCreationEvent_WhenInsideContainer_ShouldCaptureContainer()
    {
        var container = Container();

        var itemEvent = ItemEvent.NewCreationEvent(EventArticle(), container, Qty(5));

        Assert.Equal(ItemEventType.Creation, itemEvent.EventType);
        Assert.Equal(container.Guid, itemEvent.Lifecycle!.ItemContainerGuid);
        Assert.False(itemEvent.Lifecycle.NoContainer);
    }

    [Fact]
    public void NewDeletionEvent_WhenValid_ShouldRecordQuantityAndContainer()
    {
        var article = EventArticle();
        var container = Container();

        var itemEvent = ItemEvent.NewDeletionEvent(article, container, Qty(3));

        Assert.Equal(article.Guid, itemEvent.ArticleGuid);
        Assert.Equal(ItemEventType.Deletion, itemEvent.EventType);
        Assert.Equal(3, itemEvent.Lifecycle!.Quantity.Amount);
        Assert.Equal(container.Guid, itemEvent.Lifecycle.ItemContainerGuid);
    }

    [Fact]
    public void NewCreationEvent_WhenArticleIsEntityTracked_ShouldThrow()
    {
        Assert.Throws<FargoCoreException>(
            () => ItemEvent.NewCreationEvent(EntityArticle(), null, Qty(1)));
    }

    [Fact]
    public void NewCreationEvent_WhenQuantityNotPositive_ShouldThrow()
    {
        Assert.Throws<FargoCoreException>(
            () => ItemEvent.NewCreationEvent(EventArticle(), null, Qty(0)));
    }

    [Fact]
    public void NewDeletionEvent_WhenQuantityNotPositive_ShouldThrow()
    {
        Assert.Throws<FargoCoreException>(
            () => ItemEvent.NewDeletionEvent(EventArticle(), null, Qty(-1)));
    }

    [Fact]
    public void NewCreationEvent_WhenContainerIsNotAContainerArticle_ShouldThrow()
    {
        var notAContainer = Item.CreateItem(Article.NewArticle(new Name("Loose item")));

        Assert.Throws<FargoCoreException>(
            () => ItemEvent.NewCreationEvent(EventArticle(), notAContainer, Qty(1)));
    }
}
