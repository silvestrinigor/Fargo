using Fargo.Domain.Articles;
using Fargo.Domain.Barcodes;

namespace Fargo.Domain.Tests.Barcodes;

public sealed class ArticleBarcodesTests
{
    [Fact]
    public void Ean13_Should_AddBarcodeToBackingCollection()
    {
        // Arrange
        var article = CreateArticle();

        // Act
        article.Ean13 = new Ean13("1234567890123");

        // Assert
        Assert.Equal("1234567890123", article.Ean13?.Code);
    }

    [Fact]
    public void SettingBarcodeToNull_Should_RemoveBarcodeFromBackingCollection()
    {
        // Arrange
        var article = CreateArticle();
        article.UpcA = new UpcA("123456789012");

        // Act
        article.UpcA = null;

        // Assert
        Assert.Null(article.UpcA);
    }

    [Fact]
    public void Setting_Should_ReplaceExistingBarcodeForSameFormat()
    {
        // Arrange
        var article = CreateArticle();
        article.Ean8 = new Ean8("12345670");

        // Act
        article.Ean8 = new Ean8("12345671");

        // Assert
        Assert.Equal("12345671", article.Ean8?.Code);
    }

    [Fact]
    public void SettingToNull_Should_RemoveBarcode()
    {
        // Arrange
        var article = CreateArticle();
        article.Code128 = new Code128("ABC-123");

        // Act
        article.Code128 = null;

        // Assert
        Assert.Null(article.Code128);
    }

    [Fact]
    public void Article_Should_ReplaceBarcodes()
    {
        // Arrange
        var article = CreateArticle();

        var newEan13 = new Ean13("1234567890123");
        var newUpcA = new UpcA("123456789012");

        // Act
        article.Ean13 = newEan13;
        article.UpcA = newUpcA;

        // Assert
        Assert.Equal("1234567890123", article.Ean13?.Code);
        Assert.Equal("123456789012", article.UpcA?.Code);
    }

    private static Article CreateArticle() => new()
    {
        Name = new Name("Test article"),
        Description = new Description("Test description")
    };
}
