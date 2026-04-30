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
        Assert.Single(article.BarcodeCollection);
        Assert.Equal(BarcodeFormat.Ean13, article.BarcodeCollection[0].Format);
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
        Assert.Empty(article.BarcodeCollection);
    }

    [Fact]
    public void Set_Should_ReplaceExistingBarcodeForSameFormat()
    {
        // Arrange
        var article = CreateArticle();
        article.Barcodes.Ean8 = new Ean8("12345670");

        // Act
        article.Barcodes.Set(BarcodeValue.Ean8("12345671"));

        // Assert
        Assert.Equal("12345671", article.Barcodes.Ean8?.Code);
        Assert.Single(article.BarcodeCollection);
    }

    [Fact]
    public void Remove_Should_RemoveBarcodeByGuid()
    {
        // Arrange
        var article = CreateArticle();
        var barcode = article.Barcodes.Set(BarcodeValue.Code128("ABC-123"));

        // Act
        var removed = article.Barcodes.Remove(barcode.Guid);

        // Assert
        Assert.True(removed);
        Assert.Null(article.Barcodes.Code128);
        Assert.Empty(article.BarcodeCollection);
    }

    private static Article CreateArticle() => new()
    {
        Name = new Name("Test article"),
        Description = new Description("Test description")
    };
}
