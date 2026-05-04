namespace Fargo.Domain.Barcodes;

/// <summary>UPC-E barcode value assigned to an article. Code must be exactly 8 digits.</summary>
public readonly struct UpcE : IEquatable<UpcE>
{
    public const int CodeLength = 8;

    private readonly string code;

    /// <exception cref="ArgumentException">Thrown when the value is not exactly 8 ASCII digits.</exception>
    public UpcE(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length != CodeLength || !value.All(char.IsAsciiDigit))
        {
            throw new ArgumentException($"UPC-E code must be exactly {CodeLength} digits.", nameof(value));
        }

        code = value;
    }

    private UpcE(string value, bool _) => code = value;

    /// <summary>Reconstructs a <see cref="UpcE"/> from stored data, bypassing validation.</summary>
    public static UpcE FromStorage(string value) => new(value, false);

    /// <summary>Gets the barcode code string.</summary>
    public string Code => code ?? throw new InvalidOperationException("UpcE not initialized.");

    /// <inheritdoc />
    public bool Equals(UpcE other) => string.Equals(code, other.code, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is UpcE other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => code is null ? 0 : code.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Code;

    /// <summary>Determines whether two <see cref="UpcE"/> instances are equal.</summary>
    public static bool operator ==(UpcE left, UpcE right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="UpcE"/> instances are not equal.</summary>
    public static bool operator !=(UpcE left, UpcE right) => !left.Equals(right);

    /// <summary>Explicitly converts a string to a <see cref="UpcE"/>.</summary>
    public static explicit operator UpcE(string value) => new(value);

    /// <summary>Implicitly converts a <see cref="UpcE"/> to its string representation.</summary>
    public static implicit operator string(UpcE barcode) => barcode.Code;
}
