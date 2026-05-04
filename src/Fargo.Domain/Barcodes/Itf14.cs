namespace Fargo.Domain.Barcodes;

/// <summary>ITF-14 barcode value assigned to an article. Code must be exactly 14 digits.</summary>
public readonly struct Itf14 : IEquatable<Itf14>
{
    public const int CodeLength = 14;

    private readonly string code;

    /// <exception cref="ArgumentException">Thrown when the value is not exactly 14 ASCII digits.</exception>
    public Itf14(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length != CodeLength || !value.All(char.IsAsciiDigit))
        {
            throw new ArgumentException($"ITF-14 code must be exactly {CodeLength} digits.", nameof(value));
        }

        code = value;
    }

    private Itf14(string value, bool _) => code = value;

    /// <summary>Reconstructs an <see cref="Itf14"/> from stored data, bypassing validation.</summary>
    public static Itf14 FromStorage(string value) => new(value, false);

    /// <summary>Gets the barcode code string.</summary>
    public string Code => code ?? throw new InvalidOperationException("Itf14 not initialized.");

    /// <inheritdoc />
    public bool Equals(Itf14 other) => string.Equals(code, other.code, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Itf14 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => code is null ? 0 : code.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Code;

    /// <summary>Determines whether two <see cref="Itf14"/> instances are equal.</summary>
    public static bool operator ==(Itf14 left, Itf14 right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="Itf14"/> instances are not equal.</summary>
    public static bool operator !=(Itf14 left, Itf14 right) => !left.Equals(right);

    /// <summary>Explicitly converts a string to an <see cref="Itf14"/>.</summary>
    public static explicit operator Itf14(string value) => new(value);

    /// <summary>Implicitly converts an <see cref="Itf14"/> to its string representation.</summary>
    public static implicit operator string(Itf14 barcode) => barcode.Code;
}
