using Fargo.Application.Common;
using Fargo.Core.Articles;
using Fargo.Core.Informations;
using Fargo.Core.Items;
using Fargo.Infrastructure.Repositories;
using Fargo.Infrastructure.Tests.Common;

namespace Fargo.Infrastructure.Tests.Repositories;

public class ItemRepositoryTests
{
    private static (ArticleRepository, ItemRepository) Repos(Fargo.Infrastructure.Persistence.FargoDbContext ctx)
        => (new ArticleRepository(ctx), new ItemRepository(ctx));

    [Fact]
    public async Task Add_ThenGetByGuid_ShouldReturnItem()
    {
        await using var context = TestDbContextFactory.Create();
        var (articleRepo, itemRepo) = Repos(context);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepo.Add(article);
        var item = Item.CreateItem(article);
        itemRepo.Add(item);
        await context.SaveChangesAsync();

        var found = await itemRepo.GetByGuidAsync(item.Guid);

        Assert.NotNull(found);
        Assert.Equal(item.Guid, found!.Guid);
        Assert.Equal(article.Guid, found.ArticleGuid);
    }

    [Fact]
    public async Task GetInfoByGuid_ShouldProjectDto()
    {
        await using var context = TestDbContextFactory.Create();
        var (articleRepo, itemRepo) = Repos(context);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepo.Add(article);
        var item = Item.CreateItem(article);
        itemRepo.Add(item);
        await context.SaveChangesAsync();

        var dto = await itemRepo.GetInfoByGuidAsync(item.Guid);

        Assert.NotNull(dto);
        Assert.Equal(item.Guid, dto!.Guid);
        Assert.Equal(article.Guid, dto.ArticleGuid);
    }

    [Fact]
    public async Task GetManyInfo_ShouldPageAndReportTotal()
    {
        await using var context = TestDbContextFactory.Create();
        var (articleRepo, itemRepo) = Repos(context);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepo.Add(article);
        itemRepo.Add(Item.CreateItem(article));
        itemRepo.Add(Item.CreateItem(article));
        itemRepo.Add(Item.CreateItem(article));
        await context.SaveChangesAsync();

        var page = await itemRepo.GetManyInfoOrderedByGuidAsync(new Pagination(new Page(1), new Limit(2)));

        Assert.Equal(3, page.TotalCount);
        Assert.Equal(2, page.Items.Count);
    }

    [Fact]
    public async Task GetInventoryInfoByGuid_WhenFilteredByContainer_ShouldCountDirectChildrenByArticle()
    {
        await using var context = TestDbContextFactory.Create();
        var (articleRepo, itemRepo) = Repos(context);
        var containerArticle = Article.NewArticleContainer(new Name("Box"));
        var widgetArticle = Article.NewArticle(new Name("Widget"));
        articleRepo.Add(containerArticle);
        articleRepo.Add(widgetArticle);

        var container = Item.CreateItem(containerArticle);
        itemRepo.Add(container);
        await context.SaveChangesAsync();

        var w1 = Item.CreateItem(widgetArticle);
        var w2 = Item.CreateItem(widgetArticle);
        w1.PlaceInsideContainer(container);
        w2.PlaceInsideContainer(container);
        itemRepo.Add(w1);
        itemRepo.Add(w2);
        await context.SaveChangesAsync();

        var inventory = await itemRepo.GetInventoryInfoByGuidAsync(
            itemContainerGuids: [container.Guid]);

        var widgetRow = Assert.Single(inventory, r => r.ArticleGuid == widgetArticle.Guid);
        Assert.Equal(2, widgetRow.TotalCount);
    }

    [Fact]
    public async Task ExistByGuid_ShouldReflectPersistence()
    {
        await using var context = TestDbContextFactory.Create();
        var (articleRepo, itemRepo) = Repos(context);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepo.Add(article);
        var item = Item.CreateItem(article);
        itemRepo.Add(item);
        await context.SaveChangesAsync();

        Assert.True(await itemRepo.ExistByGuidAsync(item.Guid));
        Assert.False(await itemRepo.ExistByGuidAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Remove_ShouldDeleteItem()
    {
        await using var context = TestDbContextFactory.Create();
        var (articleRepo, itemRepo) = Repos(context);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepo.Add(article);
        var item = Item.CreateItem(article);
        itemRepo.Add(item);
        await context.SaveChangesAsync();

        itemRepo.Remove(item);
        await context.SaveChangesAsync();

        Assert.Null(await itemRepo.GetByGuidAsync(item.Guid));
    }
}
