using Fargo.Application.Common;
using Fargo.Core.Articles;
using Fargo.Core.Informations;
using Fargo.Core.Items;
using Fargo.Core.Items.Events;
using Fargo.Infrastructure.Repositories;
using Fargo.Infrastructure.Tests.Common;
using UnitsNet;

namespace Fargo.Infrastructure.Tests.Repositories;

public class ItemEventRepositoryTests
{
    private static Scalar Qty(double amount) => Scalar.FromAmount(amount);

    private static (ArticleRepository, ItemRepository, ItemEventRepository) Repos(
        Fargo.Infrastructure.Persistence.FargoDbContext ctx)
        => (new ArticleRepository(ctx), new ItemRepository(ctx), new ItemEventRepository(ctx));

    [Fact]
    public async Task GetMany_ShouldReturnEventsForArticle_OrderedByOccuredAt()
    {
        await using var context = TestDbContextFactory.Create();
        var (articleRepo, itemRepo, eventRepo) = Repos(context);

        var article = Article.NewArticle(new Name("Bulk sand"), ArticleTrackingMode.Event);
        articleRepo.Add(article);

        var container = Item.CreateItem(Article.NewArticleContainer(new Name("Silo")));
        itemRepo.Add(container);

        var older = ItemEvent.NewMovimentEvent(
            article, null, container, Qty(10), DateTimeOffset.UtcNow.AddHours(-2));
        var newer = ItemEvent.NewMovimentEvent(
            article, container, null, Qty(4), DateTimeOffset.UtcNow.AddHours(-1));
        eventRepo.Add(newer);
        eventRepo.Add(older);
        await context.SaveChangesAsync();

        var page = await eventRepo.GetManyInfoOrderedByOccuredAtAsync(
            new Pagination(new Page(1), new Limit(10)),
            articleGuid: article.Guid);

        Assert.Equal(2, page.TotalCount);
        Assert.Equal(2, page.Items.Count);
        // Ordered oldest-first.
        Assert.True(page.Items.First().OccuredAt <= page.Items.Last().OccuredAt);
        Assert.All(page.Items, e => Assert.Equal(article.Guid, e.ArticleGuid));
        Assert.All(page.Items, e => Assert.Equal(ItemEventType.Moviment, e.EventType));
        Assert.All(page.Items, e => Assert.NotNull(e.Moviment));
    }

    [Fact]
    public async Task GetMany_ShouldFilterByPeriod()
    {
        await using var context = TestDbContextFactory.Create();
        var (articleRepo, itemRepo, eventRepo) = Repos(context);

        var article = Article.NewArticle(new Name("Bulk sand"), ArticleTrackingMode.Event);
        articleRepo.Add(article);
        var container = Item.CreateItem(Article.NewArticleContainer(new Name("Silo")));
        itemRepo.Add(container);

        var old = ItemEvent.NewMovimentEvent(
            article, null, container, Qty(10), DateTimeOffset.UtcNow.AddDays(-10));
        var recent = ItemEvent.NewMovimentEvent(
            article, null, container, Qty(5), DateTimeOffset.UtcNow.AddDays(-1));
        eventRepo.Add(old);
        eventRepo.Add(recent);
        await context.SaveChangesAsync();

        var page = await eventRepo.GetManyInfoOrderedByOccuredAtAsync(
            new Pagination(new Page(1), new Limit(10)),
            periodStart: DateTimeOffset.UtcNow.AddDays(-2));

        Assert.Equal(1, page.TotalCount);
        Assert.Equal(5, page.Items.Single().Moviment!.Quantity.Amount);
    }
}
