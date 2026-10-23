using Fargo.Core.Barcodes;

namespace Fargo.Core.Tests.Barcodes;

public class Ean13Tests
{
    [Fact]
    public void Constructor_WhenThirteenDigits_ShouldSetValue()
    {
        var ean = new Ean13("4006381333931");

        Assert.Equal("4006381333931", ean.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WhenNullEmptyOrWhitespace_ShouldThrow(string value)
    {
        Assert.Throws<ArgumentException>(() => new Ean13(value));
    }

    [Theory]
    [InlineData("123")]               // too short
    [InlineData("12345678901234")]    // too long (14)
    [InlineData("400638133393X")]     // non-digit
    [InlineData("400638133393 ")]     // trailing space, not a digit
    public void Constructor_WhenNotExactlyThirteenDigits_ShouldThrow(string value)
    {
        Assert.Throws<ArgumentException>(() => new Ean13(value));
    }

    [Fact]
    public void Equality_WhenSameValue_ShouldBeEqual()
    {
        var a = new Ean13("4006381333931");
        var b = new Ean13("4006381333931");

        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_WhenDifferentValue_ShouldNotBeEqual()
    {
        var a = new Ean13("4006381333931");
        var b = new Ean13("0000000000000");

        Assert.True(a != b);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void ImplicitStringConversion_ShouldReturnValue()
    {
        var ean = new Ean13("4006381333931");

        string value = ean;

        Assert.Equal("4006381333931", value);
    }

    [Fact]
    public void ExplicitStringConversion_ShouldCreateEan13()
    {
        var ean = (Ean13)"4006381333931";

        Assert.Equal("4006381333931", ean.Value);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var ean = new Ean13("4006381333931");

        Assert.Equal("4006381333931", ean.ToString());
    }
}
