using Fargo.Application.Common;
using Fargo.Core.Articles;
using Fargo.Core.Informations;
using Fargo.Core.Items;
using Fargo.Infrastructure.Repositories;
using Fargo.Infrastructure.Tests.Common;

namespace Fargo.Infrastructure.Tests.Repositories;

public class ArticleRepositoryTests
{
    [Fact]
    public async Task Add_ThenGetByGuid_ShouldReturnArticle()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new ArticleRepository(context);
        var article = Article.NewArticle(new Name("Widget"));
        repo.Add(article);
        await context.SaveChangesAsync();

        var found = await repo.GetByGuidAsync(article.Guid);

        Assert.NotNull(found);
        Assert.Equal(article.Guid, found!.Guid);
    }

    [Fact]
    public async Task HasItemsAssociated_ShouldReflectItems()
    {
        await using var context = TestDbContextFactory.Create();
        var articleRepo = new ArticleRepository(context);
        var itemRepo = new ItemRepository(context);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepo.Add(article);
        await context.SaveChangesAsync();

        Assert.False(await articleRepo.HasItemsAssociatedAsync(article.Guid));

        itemRepo.Add(Item.CreateItem(article));
        await context.SaveChangesAsync();

        Assert.True(await articleRepo.HasItemsAssociatedAsync(article.Guid));
    }

    [Fact]
    public async Task IsDependencyOfAnotherArticle_ShouldDetectVariationSource()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new ArticleRepository(context);
        var source = Article.NewArticle(new Name("Source"));
        var variation = Article.NewArticleVariation(new Name("Variation"), source);
        repo.Add(source);
        repo.Add(variation);
        await context.SaveChangesAsync();

        Assert.True(await repo.IsDependencyOfAnotherArticleAsync(source.Guid));
        Assert.False(await repo.IsDependencyOfAnotherArticleAsync(variation.Guid));
    }

    [Fact]
    public async Task GetInfoByGuid_ShouldProjectDto()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new ArticleRepository(context);
        var article = Article.NewArticle(new Name("Widget"));
        repo.Add(article);
        await context.SaveChangesAsync();

        var dto = await repo.GetInfoByGuidAsync(article.Guid);

        Assert.NotNull(dto);
        Assert.Equal(article.Guid, dto!.Guid);
    }

    [Fact]
    public async Task GetManyInfo_ShouldPageAndReportTotal()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new ArticleRepository(context);
        repo.Add(Article.NewArticle(new Name("A1")));
        repo.Add(Article.NewArticle(new Name("A2")));
        repo.Add(Article.NewArticle(new Name("A3")));
        await context.SaveChangesAsync();

        var page = await repo.GetManyInfoOrderedByGuidAsync(new Pagination(new Page(1), new Limit(2)));

        Assert.Equal(3, page.TotalCount);
        Assert.Equal(2, page.Items.Count);
    }

    [Fact]
    public async Task GetInventoryInfoByGuid_ShouldCountItemsOfArticle()
    {
        await using var context = TestDbContextFactory.Create();
        var articleRepo = new ArticleRepository(context);
        var itemRepo = new ItemRepository(context);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepo.Add(article);
        itemRepo.Add(Item.CreateItem(article));
        itemRepo.Add(Item.CreateItem(article));
        await context.SaveChangesAsync();

        var inventory = await articleRepo.GetInventoryInfoByGuidAsync(article.Guid);

        Assert.Equal(2, inventory.TotalCount);
    }

    [Fact]
    public async Task ExistByGuid_ShouldReflectPersistence()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new ArticleRepository(context);
        var article = Article.NewArticle(new Name("Widget"));
        repo.Add(article);
        await context.SaveChangesAsync();

        Assert.True(await repo.ExistByGuidAsync(article.Guid));
        Assert.False(await repo.ExistByGuidAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Remove_ShouldDeleteArticle()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new ArticleRepository(context);
        var article = Article.NewArticle(new Name("Widget"));
        repo.Add(article);
        await context.SaveChangesAsync();

        repo.Remove(article);
        await context.SaveChangesAsync();

        Assert.Null(await repo.GetByGuidAsync(article.Guid));
    }
}
