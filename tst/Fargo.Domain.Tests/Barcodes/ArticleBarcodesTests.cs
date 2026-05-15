using Fargo.Core.Articles;
using Fargo.Core.Barcodes;
using Fargo.Core.Identity;
using NSubstitute;

namespace Fargo.Core.Tests.Barcodes;

public sealed class ArticleBarcodesTests
{
    [Fact]
    public async Task Ean13_Should_AddBarcodeToBackingCollection()
    {
        // Arrange
        var article = CreateArticle();
        var service = CreateService();

        // Act
        article.StartEdit(TestActor.Instance);
        await service.SetEan13(new Ean13("1234567890123"), article);

        // Assert
        Assert.Equal("1234567890123", article.Ean13?.Code);
    }

    [Fact]
    public async Task SettingBarcodeToNull_Should_RemoveBarcodeFromBackingCollection()
    {
        // Arrange
        var article = CreateArticle();
        var service = CreateService();
        article.StartEdit(TestActor.Instance);
        await service.SetUpcA(new UpcA("123456789012"), article);

        // Act
        await service.SetUpcA(null, article);

        // Assert
        Assert.Null(article.UpcA);
    }

    [Fact]
    public async Task Setting_Should_ReplaceExistingBarcodeForSameFormat()
    {
        // Arrange
        var article = CreateArticle();
        var service = CreateService();
        article.StartEdit(TestActor.Instance);
        await service.SetEan8(new Ean8("12345670"), article);

        // Act
        await service.SetEan8(new Ean8("12345671"), article);

        // Assert
        Assert.Equal("12345671", article.Ean8?.Code);
    }

    [Fact]
    public async Task SettingToNull_Should_RemoveBarcode()
    {
        // Arrange
        var article = CreateArticle();
        var service = CreateService();
        article.StartEdit(TestActor.Instance);
        await service.SetCode128(new Code128("ABC-123"), article);

        // Act
        await service.SetCode128(null, article);

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
        article.StartEdit(TestActor.Instance);
        await service.SetEan13(newEan13, article);
        await service.SetUpcA(newUpcA, article);

        // Assert
        Assert.Equal("1234567890123", article.Ean13?.Code);
        Assert.Equal("123456789012", article.UpcA?.Code);
    }

    private static Article CreateArticle()
    {
        var article = new Article(new Name("Test article"), TestActor.Instance);

        article.ChangeDescription(new Description("Test description"));

        return article;
    }

    private static ArticleService CreateService()
        => new(Substitute.For<IArticleRepository>());

    private static class TestActor
    {
        public static readonly Actor Instance = new(
            Guid.NewGuid(),
            isAdmin: false,
            isActive: true,
            permissionActions:
            [
                ActionType.CreateArticle,
                ActionType.EditArticle,
                ActionType.AddBarcode,
                ActionType.RemoveBarcode
            ],
            partitionAccessesGuids: []);
    }
}
