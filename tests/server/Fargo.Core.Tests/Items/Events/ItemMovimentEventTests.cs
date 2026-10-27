using Fargo.Core.Articles;
using Fargo.Core.Common;
using Fargo.Core.Informations;
using Fargo.Core.Items;
using Fargo.Core.Items.Events;
using UnitsNet;

namespace Fargo.Core.Tests.Items.Events;

public class ItemMovimentEventTests
{
    private static Article EventArticle() =>
        Article.NewArticle(new Name("Bulk sand"), ArticleTrackingMode.Event);

    private static Article EntityArticle() =>
        Article.NewArticle(new Name("Widget"), ArticleTrackingMode.Entity);

    private static Item Container() =>
        Item.CreateItem(Article.NewArticleContainer(new Name("Silo")));

    private static Scalar Qty(double amount) => Scalar.FromAmount(amount);

    [Fact]
    public void NewMovimentEvent_WhenReceiptIntoContainer_ShouldCapturePayload()
    {
        var article = EventArticle();
        var to = Container();

        var itemEvent = ItemEvent.NewMovimentEvent(article, null, to, Qty(50));

        Assert.Equal(article.Guid, itemEvent.ArticleGuid);
        Assert.Equal(ItemEventType.Moviment, itemEvent.EventType);

        var moviment = itemEvent.Moviment;
        Assert.NotNull(moviment);
        Assert.Equal(itemEvent.Guid, moviment!.ItemEventGuid);
        Assert.Equal(50, moviment.Quantity.Amount);
        Assert.True(moviment.FromNoContainer);
        Assert.Null(moviment.FromItemContainerGuid);
        Assert.Equal(to.Guid, moviment.ToItemContainerGuid);
        Assert.False(moviment.ToNoContainer);
    }

    [Fact]
    public void NewMovimentEvent_WhenOccuredAtProvided_ShouldBackdate()
    {
        var at = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var itemEvent = ItemEvent.NewMovimentEvent(EventArticle(), null, Container(), Qty(1), at);

        Assert.Equal(at, itemEvent.OccuredAt);
    }

    [Fact]
    public void NewMovimentEvent_WhenArticleIsEntityTracked_ShouldThrow()
    {
        Assert.Throws<FargoCoreException>(
            () => ItemEvent.NewMovimentEvent(EntityArticle(), null, Container(), Qty(1)));
    }

    [Fact]
    public void NewMovimentEvent_WhenQuantityNotPositive_ShouldThrow()
    {
        Assert.Throws<FargoCoreException>(
            () => ItemEvent.NewMovimentEvent(EventArticle(), null, Container(), Qty(0)));
    }

    [Fact]
    public void NewMovimentEvent_WhenNoOriginNorDestination_ShouldThrow()
    {
        Assert.Throws<FargoCoreException>(
            () => ItemEvent.NewMovimentEvent(EventArticle(), null, null, Qty(1)));
    }

    [Fact]
    public void NewMovimentEvent_WhenOriginEqualsDestination_ShouldThrow()
    {
        var container = Container();

        Assert.Throws<FargoCoreException>(
            () => ItemEvent.NewMovimentEvent(EventArticle(), container, container, Qty(1)));
    }

    [Fact]
    public void NewMovimentEvent_WhenContainerIsNotAContainerArticle_ShouldThrow()
    {
        var notAContainer = Item.CreateItem(Article.NewArticle(new Name("Loose item")));

        Assert.Throws<FargoCoreException>(
            () => ItemEvent.NewMovimentEvent(EventArticle(), null, notAContainer, Qty(1)));
    }

    [Fact]
    public void CreateItem_WhenArticleIsEventTracked_ShouldThrow()
    {
        Assert.Throws<FargoCoreException>(() => Item.CreateItem(EventArticle()));
    }
}
