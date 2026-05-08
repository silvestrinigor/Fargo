using Fargo.Sdk.Contracts.Articles;

namespace Fargo.Sdk.Tests.Articles;

public sealed class ArticleBarcodeTests
{
    [Fact]
    public void TryParse_Should_ParseBarcodeAndType()
    {
        var parsed = ArticleBarcode.TryParse("7891234567895:Ean13", null, out var articleBarcode);

        Assert.True(parsed);
        Assert.Equal("7891234567895", articleBarcode.Barcode);
        Assert.Equal(ArticleBarcodeType.Ean13, articleBarcode.Type);
    }

    [Fact]
    public void TryParse_Should_UseLastColonAsTypeSeparator()
    {
        var parsed = ArticleBarcode.TryParse("prefix:payload:QrCode", null, out var articleBarcode);

        Assert.True(parsed);
        Assert.Equal("prefix:payload", articleBarcode.Barcode);
        Assert.Equal(ArticleBarcodeType.QrCode, articleBarcode.Type);
    }

    [Theory]
    [InlineData("")]
    [InlineData("7891234567895")]
    [InlineData(":Ean13")]
    [InlineData("7891234567895:")]
    [InlineData("7891234567895:Unknown")]
    public void TryParse_Should_RejectInvalidRouteValues(string value)
    {
        var parsed = ArticleBarcode.TryParse(value, null, out _);

        Assert.False(parsed);
    }
}
