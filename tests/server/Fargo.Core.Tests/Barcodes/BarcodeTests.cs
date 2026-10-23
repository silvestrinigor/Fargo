using Fargo.Core.Barcodes;

namespace Fargo.Core.Tests.Barcodes;

public class BarcodeTests
{
    [Fact]
    public void DefaultConstructor_ShouldBeNoneFormat()
    {
        var barcode = new Barcode();

        Assert.Equal(BarcodeFormat.None, barcode.BarcodeFormat);
        Assert.IsType<BarcodeNone>(barcode.Value);
    }

    [Fact]
    public void Parse_WhenValidEan13_ShouldReturnBarcode()
    {
        var barcode = Barcode.Parse("4006381333931:ean13", null);

        Assert.Equal(BarcodeFormat.Ean13, barcode.BarcodeFormat);
        Assert.Equal("4006381333931", barcode.Value.ToString());
    }

    [Fact]
    public void Parse_IsCaseInsensitiveOnFormat()
    {
        var barcode = Barcode.Parse("4006381333931:EAN13", null);

        Assert.Equal(BarcodeFormat.Ean13, barcode.BarcodeFormat);
    }

    [Fact]
    public void ToString_ShouldRoundTripThroughParse()
    {
        var original = Barcode.Parse("4006381333931:ean13", null);

        var text = original.ToString();
        var parsed = Barcode.Parse(text, null);

        Assert.Equal("4006381333931:ean13", text);
        Assert.Equal(original, parsed);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("noseparator")]
    [InlineData(":ean13")]              // empty value
    [InlineData("4006381333931:")]      // empty format
    [InlineData("4006381333931:bogus")] // unknown format
    public void TryParse_WhenInvalid_ShouldReturnFalse(string? input)
    {
        var ok = Barcode.TryParse(input, null, out _);

        Assert.False(ok);
    }

    [Fact]
    public void TryParse_WhenFormatValidButValueMalformed_PropagatesException()
    {
        // Documents current behavior: BarcodeFactory.TryCreate delegates value
        // validation to the Ean13 constructor, which throws rather than returning
        // false, so TryParse is not exception-safe for a malformed EAN-13 value.
        Assert.Throws<ArgumentException>(() => Barcode.TryParse("123:ean13", null, out _));
    }

    [Fact]
    public void Parse_WhenInvalid_ShouldThrowFormatException()
    {
        Assert.Throws<FormatException>(() => Barcode.Parse("nope", null));
    }

    [Fact]
    public void Equality_WhenSameValueAndFormat_ShouldBeEqual()
    {
        var a = Barcode.Parse("4006381333931:ean13", null);
        var b = Barcode.Parse("4006381333931:ean13", null);

        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_WhenDifferentValue_ShouldNotBeEqual()
    {
        var a = Barcode.Parse("4006381333931:ean13", null);
        var b = Barcode.Parse("0000000000000:ean13", null);

        Assert.True(a != b);
    }
}

public class BarcodeFactoryTests
{
    [Fact]
    public void TryCreate_WhenEan13_ShouldCreateEan13()
    {
        var ok = BarcodeFactory.TryCreate(BarcodeFormat.Ean13, "4006381333931", out var barcode);

        Assert.True(ok);
        Assert.IsType<Ean13>(barcode);
        Assert.Equal("4006381333931", barcode.ToString());
    }

    [Fact]
    public void TryCreate_WhenUnsupportedFormat_ShouldReturnFalse()
    {
        var ok = BarcodeFactory.TryCreate(BarcodeFormat.None, "4006381333931", out var barcode);

        Assert.False(ok);
        Assert.Null(barcode);
    }

    [Fact]
    public void TryCreate_WhenEan13ValueInvalid_ShouldThrow()
    {
        // The factory delegates validation to the Ean13 constructor.
        Assert.Throws<ArgumentException>(
            () => BarcodeFactory.TryCreate(BarcodeFormat.Ean13, "123", out _));
    }
}
