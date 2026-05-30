namespace Fargo.Core.Shared.Barcodes;

/// <summary>
/// Barcode value with its symbology.
/// </summary>
public readonly struct Barcode : IEquatable<Barcode>, IParsable<Barcode>
{
    private readonly string code;
    private readonly BarcodeFormat format;

    /// <exception cref="ArgumentException">
    /// Thrown when the value is invalid for the provided format.
    /// </exception>
    public Barcode(string code, BarcodeFormat format)
    {
        Validate(code, format);

        this.code = code;
        this.format = format;
    }

    /// <summary>Gets the barcode code string.</summary>
    public string Code => code ?? throw new InvalidOperationException("Barcode not initialized.");

    /// <summary>Gets the barcode format.</summary>
    public BarcodeFormat Format => code is null
        ? throw new InvalidOperationException("Barcode not initialized.")
        : format;

    internal void EnsureFormat(BarcodeFormat expected, string paramName)
    {
        if (Format != expected)
        {
            throw new ArgumentException($"Barcode format must be {expected}.", paramName);
        }
    }

    /// <inheritdoc />
    public bool Equals(Barcode other)
        => format == other.format && string.Equals(code, other.code, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Barcode other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(format, code is null ? 0 : code.GetHashCode(StringComparison.Ordinal));

    /// <inheritdoc />
    public override string ToString() => Code;

    /// <summary>Parses a barcode route value in the format <c>{code}:{format}</c>.</summary>
    public static Barcode Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out var result))
        {
            return result;
        }

        throw new FormatException($"Invalid barcode value: '{s}'. Expected '{{code}}:{{format}}'.");
    }

    /// <summary>Tries to parse a barcode route value in the format <c>{code}:{format}</c>.</summary>
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

    /// <summary>Determines whether two <see cref="Barcode"/> instances are equal.</summary>
    public static bool operator ==(Barcode left, Barcode right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="Barcode"/> instances are not equal.</summary>
    public static bool operator !=(Barcode left, Barcode right) => !left.Equals(right);

    private static void Validate(string code, BarcodeFormat format)
    {
        switch (format)
        {
            case BarcodeFormat.Ean13:
                _ = new Ean13(code);
                break;
            case BarcodeFormat.Ean8:
                _ = new Ean8(code);
                break;
            case BarcodeFormat.UpcA:
                _ = new UpcA(code);
                break;
            case BarcodeFormat.UpcE:
                _ = new UpcE(code);
                break;
            case BarcodeFormat.Code128:
                _ = new Code128(code);
                break;
            case BarcodeFormat.Code39:
                _ = new Code39(code);
                break;
            case BarcodeFormat.Itf14:
                _ = new Itf14(code);
                break;
            case BarcodeFormat.Gs1128:
                _ = new Gs1128(code);
                break;
            case BarcodeFormat.QrCode:
                _ = new QrCode(code);
                break;
            case BarcodeFormat.DataMatrix:
                _ = new DataMatrix(code);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported barcode format.");
        }
    }
}
