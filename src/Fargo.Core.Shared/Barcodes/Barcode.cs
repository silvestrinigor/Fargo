namespace Fargo.Core.Shared.Barcodes;

public readonly struct Barcode : IParsable<Barcode>, IEquatable<Barcode>
{
    public IBarcode Value { get; } = new BarcodeNone();

    public BarcodeFormat BarcodeFormat { get; } = BarcodeFormat.None;

    public Barcode(IBarcode value, BarcodeFormat format)
    {
        ArgumentNullException.ThrowIfNull(value);

        Value = value;
        BarcodeFormat = format;
    }

    public override string ToString() => $"{Value}:{BarcodeFormat}";

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
        => Value.GetHashCode();

    public static bool operator ==(Barcode left, Barcode right)
        => left.Equals(right);

    public static bool operator !=(Barcode left, Barcode right)
        => !left.Equals(right);
}
