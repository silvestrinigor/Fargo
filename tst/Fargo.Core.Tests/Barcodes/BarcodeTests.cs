using Fargo.Core.Shared.Barcodes;

namespace Fargo.Core.Tests.Barcodes;

public sealed class BarcodeTests
{
    [Theory]
    [MemberData(nameof(ValidBarcodes))]
    public void Constructor_Should_AcceptValidCodeForFormat(string code, BarcodeFormat format)
    {
        var barcode = new Barcode(code, format);

        Assert.Equal(code, barcode.Code);
        Assert.Equal(format, barcode.Format);
        Assert.Equal(code, barcode.ToString());
    }

    [Theory]
    [InlineData("123", BarcodeFormat.Ean13)]
    [InlineData("1234567890123", BarcodeFormat.Ean8)]
    [InlineData("abc", BarcodeFormat.UpcA)]
    [InlineData("", BarcodeFormat.Code128)]
    public void Constructor_Should_RejectInvalidCodeForFormat(string code, BarcodeFormat format)
        => Assert.Throws<ArgumentException>(() => new Barcode(code, format));

    [Fact]
    public void Parse_Should_ReadCodeAndFormat()
    {
        var barcode = Barcode.Parse("1234567890123:Ean13", null);

        Assert.Equal("1234567890123", barcode.Code);
        Assert.Equal(BarcodeFormat.Ean13, barcode.Format);
    }

    [Fact]
    public void TryParse_Should_ReadCaseInsensitiveFormat()
    {
        var parsed = Barcode.TryParse("12345670:upce", null, out var barcode);

        Assert.True(parsed);
        Assert.Equal("12345670", barcode.Code);
        Assert.Equal(BarcodeFormat.UpcE, barcode.Format);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234567890123")]
    [InlineData("1234567890123:")]
    [InlineData(":Ean13")]
    [InlineData("1234567890123:Unknown")]
    [InlineData("123:Ean13")]
    public void TryParse_Should_RejectInvalidRouteValues(string value)
    {
        var parsed = Barcode.TryParse(value, null, out _);

        Assert.False(parsed);
    }

    [Fact]
    public void Parse_Should_Throw_WhenRouteValueIsInvalid()
        => Assert.Throws<FormatException>(() => Barcode.Parse("123:Ean13", null));

    [Fact]
    public void Ean13_Should_RoundTripThroughBarcode()
    {
        var ean13 = new Ean13("1234567890123");

        var barcode = ean13.ToBarcode();
        var result = Ean13.FromBarcode(barcode);

        Assert.Equal("1234567890123", barcode.Code);
        Assert.Equal(BarcodeFormat.Ean13, barcode.Format);
        Assert.Equal(ean13, result);
    }

    [Fact]
    public void Ean13FromBarcode_Should_RejectMismatchedFormat()
    {
        var barcode = new Barcode("12345670", BarcodeFormat.Ean8);

        Assert.Throws<ArgumentException>(() => Ean13.FromBarcode(barcode));
    }

    [Theory]
    [MemberData(nameof(TypedBarcodeRoundTrips))]
    public void TypedBarcode_Should_RoundTripThroughBarcode(object typed, Barcode expected)
    {
        var barcode = typed switch
        {
            Ean13 value => value.ToBarcode(),
            Ean8 value => value.ToBarcode(),
            UpcA value => value.ToBarcode(),
            UpcE value => value.ToBarcode(),
            Code128 value => value.ToBarcode(),
            Code39 value => value.ToBarcode(),
            Itf14 value => value.ToBarcode(),
            Gs1128 value => value.ToBarcode(),
            QrCode value => value.ToBarcode(),
            DataMatrix value => value.ToBarcode(),
            _ => throw new ArgumentOutOfRangeException(nameof(typed))
        };

        Assert.Equal(expected, barcode);
    }

    public static IEnumerable<object[]> ValidBarcodes()
    {
        yield return new object[] { "1234567890123", BarcodeFormat.Ean13 };
        yield return new object[] { "12345670", BarcodeFormat.Ean8 };
        yield return new object[] { "123456789012", BarcodeFormat.UpcA };
        yield return new object[] { "12345670", BarcodeFormat.UpcE };
        yield return new object[] { "CODE-128", BarcodeFormat.Code128 };
        yield return new object[] { "CODE-39", BarcodeFormat.Code39 };
        yield return new object[] { "12345678901234", BarcodeFormat.Itf14 };
        yield return new object[] { "GS1-128", BarcodeFormat.Gs1128 };
        yield return new object[] { "https://example.test/article/1", BarcodeFormat.QrCode };
        yield return new object[] { "DM-123", BarcodeFormat.DataMatrix };
    }

    public static IEnumerable<object[]> TypedBarcodeRoundTrips()
    {
        yield return new object[] { new Ean13("1234567890123"), new Barcode("1234567890123", BarcodeFormat.Ean13) };
        yield return new object[] { new Ean8("12345670"), new Barcode("12345670", BarcodeFormat.Ean8) };
        yield return new object[] { new UpcA("123456789012"), new Barcode("123456789012", BarcodeFormat.UpcA) };
        yield return new object[] { new UpcE("12345670"), new Barcode("12345670", BarcodeFormat.UpcE) };
        yield return new object[] { new Code128("CODE-128"), new Barcode("CODE-128", BarcodeFormat.Code128) };
        yield return new object[] { new Code39("CODE-39"), new Barcode("CODE-39", BarcodeFormat.Code39) };
        yield return new object[] { new Itf14("12345678901234"), new Barcode("12345678901234", BarcodeFormat.Itf14) };
        yield return new object[] { new Gs1128("GS1-128"), new Barcode("GS1-128", BarcodeFormat.Gs1128) };
        yield return new object[] { new QrCode("https://example.test/article/1"), new Barcode("https://example.test/article/1", BarcodeFormat.QrCode) };
        yield return new object[] { new DataMatrix("DM-123"), new Barcode("DM-123", BarcodeFormat.DataMatrix) };
    }
}
