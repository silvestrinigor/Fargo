using Fargo.Domain;
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
        article.Barcodes.Ean13 = new Ean13("1234567890123");

        // Assert
        Assert.Equal("1234567890123", article.Barcodes.Ean13?.Code);
        var value = Assert.Single(article.Barcodes.AsValues());
        Assert.Equal(BarcodeFormat.Ean13, value.Format);
    }

    [Fact]
    public void SettingBarcodeToNull_Should_RemoveBarcodeFromBackingCollection()
    {
        // Arrange
        var article = CreateArticle();
        article.Barcodes.UpcA = new UpcA("123456789012");

        // Act
        article.Barcodes.UpcA = null;

        // Assert
        Assert.Null(article.Barcodes.UpcA);
        Assert.Empty(article.Barcodes.AsValues());
    }

    [Fact]
    public void Setting_Should_ReplaceExistingBarcodeForSameFormat()
    {
        // Arrange
        var article = CreateArticle();
        article.Barcodes.Ean8 = new Ean8("12345670");

        // Act
        article.Barcodes.Ean8 = new Ean8("12345671");

        // Assert
        Assert.Equal("12345671", article.Barcodes.Ean8?.Code);
        Assert.Single(article.Barcodes.AsValues());
    }

    [Fact]
    public void SettingToNull_Should_RemoveBarcode()
    {
        // Arrange
        var article = CreateArticle();
        article.Barcodes.Code128 = new Code128("ABC-123");

        // Act
        article.Barcodes.Code128 = null;

        // Assert
        Assert.Null(article.Barcodes.Code128);
        Assert.Empty(article.Barcodes.AsValues());
    }

    [Fact]
    public void Article_Should_ReplaceBarcodes()
    {
        // Arrange
        var article = CreateArticle();
        var barcodes = new ArticleBarcodes
        {
            Ean13 = new Ean13("1234567890123"),
            UpcA = new UpcA("123456789012")
        };

        // Act
        article.ReplaceBarcodes(barcodes);

        // Assert
        Assert.Equal(2, article.Barcodes.AsValues().Count());
        Assert.Equal("1234567890123", article.Barcodes.Ean13?.Code);
        Assert.Equal("123456789012", article.Barcodes.UpcA?.Code);
    }

    private static Article CreateArticle() => new()
    {
        Name = new Name("Test article"),
        Description = new Description("Test description")
    };
}
