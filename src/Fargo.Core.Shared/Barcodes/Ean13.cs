namespace Fargo.Core.Shared.Barcodes;

/// <summary>
/// EAN-13 barcode value assigned to an article.
/// Code must be exactly 13 digits.
/// </summary>
public readonly struct Ean13 : IEquatable<Ean13>
{
    /// <summary>
    /// Gets the const EAN-13 length.
    /// </summary>
    public const int CodeLength = 13;

    /// <summary>
    /// Gets the barcode code string.
    /// </summary>
    public string Code => code ?? throw new InvalidOperationException("Ean13 not initialized.");
    private readonly string? code;

    public Ean13(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Length != CodeLength || !value.All(char.IsAsciiDigit))
        {
            throw new ArgumentException($"EAN-13 code must be exactly {CodeLength} digits.", nameof(value));
        }

        code = value;
    }

    public bool Equals(Ean13 other) => string.Equals(code, other.code, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is Ean13 other && Equals(other);

    public override int GetHashCode() => code is null ? 0 : code.GetHashCode(StringComparison.Ordinal);

    public override string ToString() => Code;

    /// <summary>
    /// Determines whether two <see cref="Ean13"/> instances are equal.
    /// </summary>
    public static bool operator ==(Ean13 left, Ean13 right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Ean13"/> instances are not equal.
    /// </summary>
    public static bool operator !=(Ean13 left, Ean13 right) => !left.Equals(right);

    /// <summary>
    /// Explicitly converts a string to an <see cref="Ean13"/>.
    /// </summary>
    public static explicit operator Ean13(string value) => new(value);

    /// <summary>
    /// Implicitly converts an <see cref="Ean13"/> to its string representation.
    /// </summary>
    public static implicit operator string(Ean13 barcode) => barcode.Code;
}
