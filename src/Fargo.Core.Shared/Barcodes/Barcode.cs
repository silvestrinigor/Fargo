namespace Fargo.Core.Shared.Barcodes;

/// <summary>
/// Represents a barcode value together with its symbology format.
/// </summary>
public readonly struct Barcode : IEquatable<Barcode>, IParsable<Barcode>
{
    /// <summary>
    /// Gets the raw barcode value.
    /// </summary>
    public string Value { get; } = string.Empty;

    /// <summary>
    /// Gets the barcode format.
    /// </summary>
    public BarcodeFormat Format { get; } = BarcodeFormat.None;

    /// <summary>
    /// Initializes a new barcode value.
    /// </summary>
    /// <param name="code">The barcode value.</param>
    /// <param name="format">The barcode symbology format.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is invalid for the specified format.
    /// </exception>
    public Barcode(string code, BarcodeFormat format)
    {
        Validate(code, format);

        Value = code;
        Format = format;
    }

    public bool Equals(Barcode other)
        => Format == other.Format &&
           string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj)
        => obj is Barcode other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(
            Format, Value?.GetHashCode(StringComparison.Ordinal));

    public override string ToString() => $"{Value}:{Format}";

    public static Barcode Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out var result))
        {
            return result;
        }

        throw new FormatException($"Invalid barcode value: '{s}'. Expected '{{code}}:{{format}}'.");
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

        var code = s[..separator];
        var formatText = s[(separator + 1)..];

        if (string.IsNullOrWhiteSpace(code) ||
            !Enum.TryParse<BarcodeFormat>(formatText, ignoreCase: true, out var format))
        {
            return false;
        }

        try
        {
            result = new Barcode(code, format);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public static bool operator ==(Barcode left, Barcode right) => left.Equals(right);

    public static bool operator !=(Barcode left, Barcode right) => !left.Equals(right);

    private static void Validate(string code, BarcodeFormat format)
    {
        _ = format switch
        {
            BarcodeFormat.Ean13 => new Ean13(code),
            _ => throw new ArgumentOutOfRangeException(
                nameof(format), format, "Unsupported barcode format."),
        };
    }

    public static Barcode FromEan13(Ean13 ean13)
    {
        return new Barcode(ean13.Value, BarcodeFormat.Ean13);
    }
}
