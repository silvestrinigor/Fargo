namespace Fargo.Core.Shared.Barcodes;

/// <summary>
/// Represents a barcode value together with its symbology format.
/// </summary>
public readonly struct Barcode : IEquatable<Barcode>, IParsable<Barcode>
{
    /// <summary>
    /// Gets the raw barcode value.
    /// </summary>
    public string Code => code ?? throw new InvalidOperationException("Barcode not initialized.");
    private readonly string code;

    /// <summary>
    /// Gets the barcode format.
    /// </summary>
    public BarcodeFormat Format => code is null
        ? throw new InvalidOperationException("Barcode not initialized.")
        : format;
    private readonly BarcodeFormat format;

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

        this.code = code;
        this.format = format;
    }

    public bool Equals(Barcode other)
        => Format == other.Format &&
           string.Equals(Code, other.Code, StringComparison.Ordinal);

    public override bool Equals(object? obj)
        => obj is Barcode other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(
            format,
            code?.GetHashCode(StringComparison.Ordinal));

    public override string ToString() => $"{Code}:{Format}";

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
}
