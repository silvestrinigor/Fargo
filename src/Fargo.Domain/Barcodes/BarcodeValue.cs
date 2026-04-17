using Fargo.Domain.Enums;
using System.Text.RegularExpressions;

namespace Fargo.Domain.ValueObjects;

/// <summary>
/// Represents a barcode value combining a code string and its format (symbology).
///
/// Mirrors the UnitsNet pattern: a strongly typed value object that enforces
/// per-format validation rules. Use the public constructor or per-format static
/// factory methods to create instances. Use <see cref="FromStorage"/> to
/// reconstruct values from the database without re-validating.
/// </summary>
public readonly struct BarcodeValue : IEquatable<BarcodeValue>
{
    private static readonly Regex DigitsOnly = new(@"^\d+$", RegexOptions.Compiled);
    private static readonly Regex PrintableChars = new(@"^[\x20-\x7E]+$", RegexOptions.Compiled);

    /// <summary>
    /// Initializes a new <see cref="BarcodeValue"/>, validating the code against the given format.
    /// </summary>
    /// <param name="code">The barcode code string.</param>
    /// <param name="format">The barcode format (symbology).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="code"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="code"/> does not satisfy the format's validation rules.</exception>
    public BarcodeValue(string code, BarcodeFormat format)
    {
        ArgumentNullException.ThrowIfNull(code);
        Validate(code, format);
        Code = code;
        Format = format;
    }

    private BarcodeValue(string code, BarcodeFormat format, bool _)
    {
        Code = code;
        Format = format;
    }

    /// <summary>
    /// Reconstructs a <see cref="BarcodeValue"/> directly from stored values, bypassing validation.
    /// Used by infrastructure to rehydrate entities from the database.
    /// </summary>
    public static BarcodeValue FromStorage(string code, BarcodeFormat format) =>
        new(code, format, false);

    /// <summary>Creates an EAN-13 barcode value. Code must be exactly 13 digits.</summary>
    public static BarcodeValue Ean13(string code) => new(code, BarcodeFormat.Ean13);

    /// <summary>Creates an EAN-8 barcode value. Code must be exactly 8 digits.</summary>
    public static BarcodeValue Ean8(string code) => new(code, BarcodeFormat.Ean8);

    /// <summary>Creates a UPC-A barcode value. Code must be exactly 12 digits.</summary>
    public static BarcodeValue UpcA(string code) => new(code, BarcodeFormat.UpcA);

    /// <summary>Creates a UPC-E barcode value. Code must be exactly 8 digits.</summary>
    public static BarcodeValue UpcE(string code) => new(code, BarcodeFormat.UpcE);

    /// <summary>Creates a Code 128 barcode value. Code must be 1–80 printable ASCII characters.</summary>
    public static BarcodeValue Code128(string code) => new(code, BarcodeFormat.Code128);

    /// <summary>Creates a Code 39 barcode value. Code must be 1–80 printable ASCII characters.</summary>
    public static BarcodeValue Code39(string code) => new(code, BarcodeFormat.Code39);

    /// <summary>Creates an ITF-14 barcode value. Code must be exactly 14 digits.</summary>
    public static BarcodeValue Itf14(string code) => new(code, BarcodeFormat.Itf14);

    /// <summary>Creates a GS1-128 barcode value. Code must be 1–80 printable ASCII characters.</summary>
    public static BarcodeValue Gs1128(string code) => new(code, BarcodeFormat.Gs1128);

    /// <summary>Creates a QR Code barcode value. Code must be 1–2953 characters.</summary>
    public static BarcodeValue QrCode(string code) => new(code, BarcodeFormat.QrCode);

    /// <summary>Creates a Data Matrix barcode value. Code must be 1–2335 characters.</summary>
    public static BarcodeValue DataMatrix(string code) => new(code, BarcodeFormat.DataMatrix);

    /// <summary>Gets the barcode code string.</summary>
    public string Code { get; }

    /// <summary>Gets the barcode format (symbology).</summary>
    public BarcodeFormat Format { get; }

    /// <inheritdoc />
    public bool Equals(BarcodeValue other) =>
        string.Equals(Code, other.Code, StringComparison.Ordinal) && Format == other.Format;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is BarcodeValue other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Code, Format);

    /// <summary>Returns a string in the form <c>"Format:Code"</c>.</summary>
    public override string ToString() => $"{Format}:{Code}";

    /// <summary>Determines whether two <see cref="BarcodeValue"/> instances are equal.</summary>
    public static bool operator ==(BarcodeValue left, BarcodeValue right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="BarcodeValue"/> instances are not equal.</summary>
    public static bool operator !=(BarcodeValue left, BarcodeValue right) => !left.Equals(right);

    private static void Validate(string code, BarcodeFormat format)
    {
        switch (format)
        {
            case BarcodeFormat.Ean13:
                if (!DigitsOnly.IsMatch(code) || code.Length != 13)
                {
                    throw new ArgumentException("EAN-13 code must be exactly 13 digits.", nameof(code));
                }

                break;

            case BarcodeFormat.Ean8:
                if (!DigitsOnly.IsMatch(code) || code.Length != 8)
                {
                    throw new ArgumentException("EAN-8 code must be exactly 8 digits.", nameof(code));
                }

                break;

            case BarcodeFormat.UpcA:
                if (!DigitsOnly.IsMatch(code) || code.Length != 12)
                {
                    throw new ArgumentException("UPC-A code must be exactly 12 digits.", nameof(code));
                }

                break;

            case BarcodeFormat.UpcE:
                if (!DigitsOnly.IsMatch(code) || code.Length != 8)
                {
                    throw new ArgumentException("UPC-E code must be exactly 8 digits.", nameof(code));
                }

                break;

            case BarcodeFormat.Itf14:
                if (!DigitsOnly.IsMatch(code) || code.Length != 14)
                {
                    throw new ArgumentException("ITF-14 code must be exactly 14 digits.", nameof(code));
                }

                break;

            case BarcodeFormat.Code128:
            case BarcodeFormat.Code39:
            case BarcodeFormat.Gs1128:
                if (!PrintableChars.IsMatch(code) || code.Length is < 1 or > 80)
                {
                    throw new ArgumentException($"{format} code must be 1–80 printable ASCII characters.", nameof(code));
                }

                break;

            case BarcodeFormat.QrCode:
                if (code.Length is < 1 or > 2953)
                {
                    throw new ArgumentException("QR Code must be 1–2953 characters.", nameof(code));
                }

                break;

            case BarcodeFormat.DataMatrix:
                if (code.Length is < 1 or > 2335)
                {
                    throw new ArgumentException("Data Matrix code must be 1–2335 characters.", nameof(code));
                }

                break;

            default:
                throw new ArgumentException($"Unsupported barcode format: {format}.", nameof(format));
        }
    }
}
