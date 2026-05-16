using Fargo.Core.Articles;
using Fargo.Core.Barcodes;
using UnitsNet;

namespace Fargo.Core.Tests.Articles;

public sealed class ArticleValueObjectTests
{
    [Fact]
    public void ArticleMetrics_Should_AcceptEmptyValues()
    {
        var metrics = new ArticleMetrics();

        Assert.Null(metrics.Mass);
        Assert.Null(metrics.LengthX);
        Assert.Null(metrics.LengthY);
        Assert.Null(metrics.LengthZ);
    }

    [Fact]
    public void ArticleMetrics_Should_AcceptPositiveValues()
    {
        var metrics = new ArticleMetrics(
            Mass.FromKilograms(1),
            Length.FromCentimeters(10),
            Length.FromCentimeters(20),
            Length.FromCentimeters(30));

        Assert.Equal(Mass.FromKilograms(1), metrics.Mass);
        Assert.Equal(Length.FromCentimeters(10), metrics.LengthX);
        Assert.Equal(Length.FromCentimeters(20), metrics.LengthY);
        Assert.Equal(Length.FromCentimeters(30), metrics.LengthZ);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ArticleMetrics_Should_RejectNonPositiveMass(double kilograms)
        => Assert.Throws<ArgumentOutOfRangeException>(() => new ArticleMetrics(Mass.FromKilograms(kilograms)));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ArticleMetrics_Should_RejectNonPositiveLengths(double centimeters)
        => Assert.Throws<ArgumentOutOfRangeException>(() => new ArticleMetrics(lengthX: Length.FromCentimeters(centimeters)));

    [Fact]
    public void ArticleBarcodes_Should_HoldTypedBarcodeValues()
    {
        var ean13 = new Ean13("1234567890123");
        var code128 = new Code128("CODE-128");

        var barcodes = new ArticleBarcodesSet(Ean13: ean13, Code128: code128);

        Assert.Equal(ean13, barcodes.Ean13);
        Assert.Equal(code128, barcodes.Code128);
    }

    [Fact]
    public void ArticleBarcodes_Should_CompareByValue()
    {
        var left = new ArticleBarcodesSet(Ean13: new Ean13("1234567890123"));
        var right = new ArticleBarcodesSet(Ean13: new Ean13("1234567890123"));

        Assert.Equal(left, right);
    }
}
