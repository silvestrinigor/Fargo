using Fargo.Core.Articles;
using Fargo.Core.Shared.Barcodes;
using Fargo.Core.Shared;
using NSubstitute;

namespace Fargo.Core.Tests.Barcodes;

public sealed class ArticleBarcodesTests
{
    [Fact]
    public async Task Ean13_Should_SetBarcode()
    {
        // Arrange
        var article = CreateArticle();
        var service = CreateService();

        // Act
        await service.SetEan13(new Ean13("1234567890123"), article, CreateDomainActor());

        // Assert
        Assert.Equal("1234567890123", article.Ean13?.Code);
    }

    [Fact]
    public async Task SettingBarcodeToNull_Should_ClearBarcode()
    {
        // Arrange
        var article = CreateArticle();
        var service = CreateService();
        await service.SetUpcA(new UpcA("123456789012"), article, CreateDomainActor());

        // Act
        await service.SetUpcA(null, article, CreateDomainActor());

        // Assert
        Assert.Null(article.UpcA);
    }

    [Fact]
    public async Task Setting_Should_ReplaceExistingBarcodeForSameFormat()
    {
        // Arrange
        var article = CreateArticle();
        var service = CreateService();
        await service.SetEan8(new Ean8("12345670"), article, CreateDomainActor());

        // Act
        await service.SetEan8(new Ean8("12345671"), article, CreateDomainActor());

        // Assert
        Assert.Equal("12345671", article.Ean8?.Code);
    }

    [Fact]
    public async Task SettingToNull_Should_ClearBarcode()
    {
        // Arrange
        var article = CreateArticle();
        var service = CreateService();
        await service.SetCode128(new Code128("ABC-123"), article, CreateDomainActor());

        // Act
        await service.SetCode128(null, article, CreateDomainActor());

        // Assert
        Assert.Null(article.Code128);
    }

    [Fact]
    public async Task Article_Should_ReplaceBarcodes()
    {
        // Arrange
        var article = CreateArticle();
        var service = CreateService();

        var newEan13 = new Ean13("1234567890123");
        var newUpcA = new UpcA("123456789012");

        // Act
        await service.SetEan13(newEan13, article, CreateDomainActor());
        await service.SetUpcA(newUpcA, article, CreateDomainActor());

        // Assert
        Assert.Equal("1234567890123", article.Ean13?.Code);
        Assert.Equal("123456789012", article.UpcA?.Code);
    }

    private static Article CreateArticle()
    {
        var article = Article.CreateArticle(new Name("Test article"), CreateDomainActor());

        article.ChangeDescription(new Description("Test description"), CreateDomainActor());

        return article;
    }

    private static ArticleService CreateService()
        => new(Substitute.For<IArticleRepository>());
}
