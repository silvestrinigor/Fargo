using Fargo.Domain.Entities;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using NSubstitute;

namespace Fargo.Domain.Tests.Services;

public sealed class ArticleServiceTests
{
    [Fact]
    public async Task ValidateArticleDelete_Should_NotThrow_When_ArticleHasNoAssociatedItems()
    {
        // Arrange
        var articleRepository = Substitute.For<IArticleRepository>();
        var articleService = new ArticleService(articleRepository);
        var article = CreateArticle();

        articleRepository
            .HasItemsAssociated(article.Guid, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        async Task act() => await articleService.ValidateArticleDelete(article);

        // Assert
        await act();
        await articleRepository.Received(1)
            .HasItemsAssociated(article.Guid, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidateArticleDelete_Should_Throw_When_ArticleHasAssociatedItems()
    {
        // Arrange
        var articleRepository = Substitute.For<IArticleRepository>();
        var articleService = new ArticleService(articleRepository);
        var article = CreateArticle();

        articleRepository
            .HasItemsAssociated(article.Guid, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        async Task act() => await articleService.ValidateArticleDelete(article);

        // Assert
        await Assert.ThrowsAsync<ArticleDeleteWithItemsAssociatedFargoDomainException>(act);

        await articleRepository.Received(1)
            .HasItemsAssociated(article.Guid, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidateArticleDelete_Should_UseProvidedCancellationToken()
    {
        // Arrange
        var articleRepository = Substitute.For<IArticleRepository>();
        var articleService = new ArticleService(articleRepository);
        var article = CreateArticle();
        var cancellationToken = new CancellationTokenSource().Token;

        articleRepository
            .HasItemsAssociated(article.Guid, cancellationToken)
            .Returns(false);

        // Act
        await articleService.ValidateArticleDelete(article, cancellationToken);

        // Assert
        await articleRepository.Received(1)
            .HasItemsAssociated(article.Guid, cancellationToken);
    }

    private static Article CreateArticle()
    {
        return new Article
        {
            Name = new("test")
        };
    }
}