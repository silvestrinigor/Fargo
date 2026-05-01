namespace Fargo.Domain.Barcodes;

/// <summary>Code 128 barcode value assigned to an article. Code must be 1–80 printable ASCII characters.</summary>
public readonly struct Code128 : IEquatable<Code128>
{
    public const int MinLength = 1;
    public const int MaxLength = 80;

    private readonly string code;

    /// <exception cref="ArgumentException">Thrown when the value is not 1–80 printable ASCII characters.</exception>
    public Code128(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length is < MinLength or > MaxLength || !value.All(c => c is >= '\x20' and <= '\x7E'))
            throw new ArgumentException($"Code 128 code must be {MinLength}–{MaxLength} printable ASCII characters.", nameof(value));
        code = value;
    }

    private Code128(string value, bool _) => code = value;

    /// <summary>Reconstructs a <see cref="Code128"/> from stored data, bypassing validation.</summary>
    public static Code128 FromStorage(string value) => new(value, false);

    /// <summary>Gets the barcode code string.</summary>
    public string Code => code ?? throw new InvalidOperationException("Code128 not initialized.");

    /// <inheritdoc />
    public bool Equals(Code128 other) => string.Equals(code, other.code, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Code128 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => code is null ? 0 : code.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Code;

    /// <summary>Determines whether two <see cref="Code128"/> instances are equal.</summary>
    public static bool operator ==(Code128 left, Code128 right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="Code128"/> instances are not equal.</summary>
    public static bool operator !=(Code128 left, Code128 right) => !left.Equals(right);

    /// <summary>Explicitly converts a string to a <see cref="Code128"/>.</summary>
    public static explicit operator Code128(string value) => new(value);

    /// <summary>Implicitly converts a <see cref="Code128"/> to its string representation.</summary>
    public static implicit operator string(Code128 barcode) => barcode.Code;
}
