using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Tests.Entities;

public sealed class ItemTests
{
    private static Article CreateArticle()
    {
        return new Article
        {
            Guid = Guid.NewGuid(),
            Name = new Name("ArticleName"),
            Description = Description.Empty
        };
    }

    [Fact]
    public void Article_Should_SetArticleGuid_When_Assigned()
    {
        // Arrange
        var article = CreateArticle();

        // Act
        var item = new Item
        {
            Article = article
        };

        // Assert
        Assert.Equal(article.Guid, item.ArticleGuid);
        Assert.Same(article, item.Article);
    }

    [Fact]
    public void ArticleGuid_Should_MatchArticleGuid()
    {
        // Arrange
        var article = CreateArticle();

        // Act
        var item = new Item
        {
            Article = article
        };

        // Assert
        Assert.Equal(item.Article.Guid, item.ArticleGuid);
    }

    [Fact]
    public void DifferentItems_Should_HaveDifferentEntityGuid()
    {
        // Arrange
        var article = CreateArticle();

        // Act
        var item1 = new Item { Article = article };
        var item2 = new Item { Article = article };

        // Assert
        Assert.NotEqual(item1.Guid, item2.Guid);
    }

    [Fact]
    public void Item_Should_ReferenceSameArticleInstance()
    {
        // Arrange
        var article = CreateArticle();

        // Act
        var item = new Item
        {
            Article = article
        };

        // Assert
        Assert.Same(article, item.Article);
    }

    [Fact]
    public void Item_Should_AllowDifferentArticles()
    {
        // Arrange
        var article1 = CreateArticle();
        var article2 = CreateArticle();

        // Act
        var item1 = new Item { Article = article1 };
        var item2 = new Item { Article = article2 };

        // Assert
        Assert.NotEqual(item1.ArticleGuid, item2.ArticleGuid);
    }
}
