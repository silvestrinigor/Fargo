using Fargo.Core.Articles;
using Fargo.Core.Items;
using Fargo.Core.Shared;

namespace Fargo.Core.Tests.Items;

public sealed class ItemMutationTests
{
    [Fact]
    public void Constructor_Should_SetItemActiveByDefault()
    {
        var item = Item.CreateItem(Article.CreateArticle(new Name("Article"), CreateDomainActor()));

        Assert.True(item.IsActive);
    }

    [Fact]
    public void CreateItem_Should_SetArticleAndProductionDate()
    {
        var article = Article.CreateArticle(new Name("Article"), CreateDomainActor());
        var productionDate = DateTimeOffset.UtcNow;

        var item = Item.CreateItem(article, productionDate);

        Assert.Equal(article, item.Article);
        Assert.Equal(article.Guid, item.ArticleGuid);
        Assert.Equal(productionDate, item.ProductionDate);
    }

    [Fact]
    public void CreateItem_Should_SetContainer_WhenArticleIsContainer()
    {
        var article = Article.CreateArticleContainer(new Name("Container article"), CreateDomainActor());

        var item = Item.CreateItem(article);

        Assert.NotNull(item.Container);
        Assert.Same(item, item.Container.Item);
    }

    [Fact]
    public void ActivateDeactivate_Should_UpdateActiveState()
    {
        var item = Item.CreateItem(Article.CreateArticle(new Name("Article"), CreateDomainActor()));

        item.Deactivate();
        Assert.False(item.IsActive);

        item.Activate();
        Assert.True(item.IsActive);
    }
}
