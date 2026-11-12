namespace Fargo.Core.Barcodes;

/// <summary>
/// A barcode value with its format. The string form is "value:format" (format in lower case);
/// the default instance is <see cref="BarcodeFormat.None"/>.
/// </summary>
public readonly struct Barcode : IParsable<Barcode>, IEquatable<Barcode>
{
    public IBarcode Value { get; }

    public BarcodeFormat BarcodeFormat { get; }

    public Barcode(IBarcode value, BarcodeFormat format)
    {
        ArgumentNullException.ThrowIfNull(value);

        Value = value;
        BarcodeFormat = format;
    }

    public Barcode()
    {
        Value = new BarcodeNone();
        BarcodeFormat = BarcodeFormat.None;
    }

    public override string ToString() => $"{Value}:{BarcodeFormat.ToString().ToLowerInvariant()}";

    public static Barcode Parse(string s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, out var result))
        {
            throw new FormatException($"Invalid barcode value: '{s}'.");
        }

        return result;
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out Barcode result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        var separator = s.LastIndexOf(':');
        if (separator <= 0 || separator == s.Length - 1)
        {
            return false;
        }

        var value = s[..separator];
        var formatText = s[(separator + 1)..];

        if (!Enum.TryParse<BarcodeFormat>(formatText, true, out var format))
        {
            return false;
        }

        if (!BarcodeFactory.TryCreate(format, value, out var barcode))
        {
            return false;
        }

        result = new Barcode(barcode, format);
        return true;
    }

    public bool Equals(Barcode other)
        => Value.Equals(other.Value) && BarcodeFormat.Equals(other.BarcodeFormat);

    public override bool Equals(object? obj)
        => obj is Barcode other && Equals(other);

    public override int GetHashCode()
    {
        return HashCode.Combine(Value, BarcodeFormat);
    }

    public static bool operator ==(Barcode left, Barcode right)
        => left.Equals(right);

    public static bool operator !=(Barcode left, Barcode right)
        => !left.Equals(right);
}
