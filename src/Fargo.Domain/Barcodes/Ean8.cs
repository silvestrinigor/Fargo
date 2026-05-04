namespace Fargo.Domain.Barcodes;

/// <summary>EAN-8 barcode value assigned to an article. Code must be exactly 8 digits.</summary>
public readonly struct Ean8 : IEquatable<Ean8>
{
    public const int CodeLength = 8;

    private readonly string code;

    /// <exception cref="ArgumentException">Thrown when the value is not exactly 8 ASCII digits.</exception>
    public Ean8(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length != CodeLength || !value.All(char.IsAsciiDigit))
        {
            throw new ArgumentException($"EAN-8 code must be exactly {CodeLength} digits.", nameof(value));
        }

        code = value;
    }

    private Ean8(string value, bool _) => code = value;

    /// <summary>Reconstructs an <see cref="Ean8"/> from stored data, bypassing validation.</summary>
    public static Ean8 FromStorage(string value) => new(value, false);

    /// <summary>Gets the barcode code string.</summary>
    public string Code => code ?? throw new InvalidOperationException("Ean8 not initialized.");

    /// <inheritdoc />
    public bool Equals(Ean8 other) => string.Equals(code, other.code, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Ean8 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => code is null ? 0 : code.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Code;

    /// <summary>Determines whether two <see cref="Ean8"/> instances are equal.</summary>
    public static bool operator ==(Ean8 left, Ean8 right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="Ean8"/> instances are not equal.</summary>
    public static bool operator !=(Ean8 left, Ean8 right) => !left.Equals(right);

    /// <summary>Explicitly converts a string to an <see cref="Ean8"/>.</summary>
    public static explicit operator Ean8(string value) => new(value);

    /// <summary>Implicitly converts an <see cref="Ean8"/> to its string representation.</summary>
    public static implicit operator string(Ean8 barcode) => barcode.Code;
}
