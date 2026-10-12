namespace Fargo.Core.Barcodes;

/// <summary>
/// EAN-13 barcode value assigned to an article.
/// Code must be exactly 13 digits.
/// </summary>
public readonly struct Ean13 : IBarcode, IEquatable<Ean13>
{
    /// <summary>
    /// Gets the const EAN-13 length.
    /// </summary>
    public const int CodeLength = 13;

    /// <summary>
    /// Gets the barcode code string.
    /// </summary>
    public string Value { get; } = "0000000000000";

    /// <summary>
    /// Initializes a new instance of the <see cref="Ean13"/> struct.
    /// </summary>
    /// <param name="value">
    /// The 13-digit numeric code of the barcode.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is null, empty, or does not consist
    /// of exactly <see cref="CodeLength"/> ASCII digits.
    /// </exception>
    public Ean13(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Length != CodeLength || !value.All(char.IsAsciiDigit))
        {
            throw new ArgumentException($"EAN-13 code must be exactly {CodeLength} digits.", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Determines whether the specified <see cref="Ean13"/> is equal to the current
    /// <see cref="Ean13"/>.
    /// </summary>
    /// <param name="other">The <see cref="Ean13"/> to compare with the current instance.</param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="Ean13"/> has the same
    /// code; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(Ean13 other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="Ean13"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>
    /// <see langword="true"/> if the specified object is an <see cref="Ean13"/> with
    /// the same code; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj) => obj is Ean13 other && Equals(other);

    /// <summary>
    /// Serves as a hash function for the <see cref="Ean13"/>.
    /// </summary>
    /// <returns>A hash code for the current <see cref="Ean13"/>.</returns>
    public override int GetHashCode() => Value is null ? 0 : Value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Returns the EAN-13 code as its string representation.
    /// </summary>
    /// <returns>The 13-digit EAN-13 code.</returns>
    public override string ToString() => Value;

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
    public static implicit operator string(Ean13 barcode) => barcode.Value;
}
