namespace Fargo.Domain.Barcodes;

/// <summary>GS1-128 barcode value assigned to an article. Code must be 1–80 printable ASCII characters.</summary>
public readonly struct Gs1128 : IEquatable<Gs1128>
{
    public const int MinLength = 1;
    public const int MaxLength = 80;

    private readonly string code;

    /// <exception cref="ArgumentException">Thrown when the value is not 1–80 printable ASCII characters.</exception>
    public Gs1128(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length is < MinLength or > MaxLength || !value.All(c => c is >= '\x20' and <= '\x7E'))
        {
            throw new ArgumentException($"GS1-128 code must be {MinLength}–{MaxLength} printable ASCII characters.", nameof(value));
        }

        code = value;
    }

    private Gs1128(string value, bool _) => code = value;

    /// <summary>Reconstructs a <see cref="Gs1128"/> from stored data, bypassing validation.</summary>
    public static Gs1128 FromStorage(string value) => new(value, false);

    /// <summary>Gets the barcode code string.</summary>
    public string Code => code ?? throw new InvalidOperationException("Gs1128 not initialized.");

    /// <inheritdoc />
    public bool Equals(Gs1128 other) => string.Equals(code, other.code, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Gs1128 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => code is null ? 0 : code.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Code;

    /// <summary>Determines whether two <see cref="Gs1128"/> instances are equal.</summary>
    public static bool operator ==(Gs1128 left, Gs1128 right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="Gs1128"/> instances are not equal.</summary>
    public static bool operator !=(Gs1128 left, Gs1128 right) => !left.Equals(right);

    /// <summary>Explicitly converts a string to a <see cref="Gs1128"/>.</summary>
    public static explicit operator Gs1128(string value) => new(value);

    /// <summary>Implicitly converts a <see cref="Gs1128"/> to its string representation.</summary>
    public static implicit operator string(Gs1128 barcode) => barcode.Code;
}
