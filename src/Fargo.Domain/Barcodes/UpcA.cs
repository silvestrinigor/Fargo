namespace Fargo.Domain.Barcodes;

/// <summary>UPC-A barcode value assigned to an article. Code must be exactly 12 digits.</summary>
public readonly struct UpcA : IEquatable<UpcA>
{
    public const int CodeLength = 12;

    private readonly string code;

    /// <exception cref="ArgumentException">Thrown when the value is not exactly 12 ASCII digits.</exception>
    public UpcA(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length != CodeLength || !value.All(char.IsAsciiDigit))
        {
            throw new ArgumentException($"UPC-A code must be exactly {CodeLength} digits.", nameof(value));
        }

        code = value;
    }

    private UpcA(string value, bool _) => code = value;

    /// <summary>Reconstructs a <see cref="UpcA"/> from stored data, bypassing validation.</summary>
    public static UpcA FromStorage(string value) => new(value, false);

    /// <summary>Gets the barcode code string.</summary>
    public string Code => code ?? throw new InvalidOperationException("UpcA not initialized.");

    /// <inheritdoc />
    public bool Equals(UpcA other) => string.Equals(code, other.code, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is UpcA other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => code is null ? 0 : code.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Code;

    /// <summary>Determines whether two <see cref="UpcA"/> instances are equal.</summary>
    public static bool operator ==(UpcA left, UpcA right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="UpcA"/> instances are not equal.</summary>
    public static bool operator !=(UpcA left, UpcA right) => !left.Equals(right);

    /// <summary>Explicitly converts a string to a <see cref="UpcA"/>.</summary>
    public static explicit operator UpcA(string value) => new(value);

    /// <summary>Implicitly converts a <see cref="UpcA"/> to its string representation.</summary>
    public static implicit operator string(UpcA barcode) => barcode.Code;
}
