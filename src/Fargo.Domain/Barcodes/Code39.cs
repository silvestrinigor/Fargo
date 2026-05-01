namespace Fargo.Domain.Barcodes;

/// <summary>Code 39 barcode value assigned to an article. Code must be 1–80 printable ASCII characters.</summary>
public readonly struct Code39 : IEquatable<Code39>
{
    public const int MinLength = 1;
    public const int MaxLength = 80;

    private readonly string code;

    /// <exception cref="ArgumentException">Thrown when the value is not 1–80 printable ASCII characters.</exception>
    public Code39(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length is < MinLength or > MaxLength || !value.All(c => c is >= '\x20' and <= '\x7E'))
            throw new ArgumentException($"Code 39 code must be {MinLength}–{MaxLength} printable ASCII characters.", nameof(value));
        code = value;
    }

    private Code39(string value, bool _) => code = value;

    /// <summary>Reconstructs a <see cref="Code39"/> from stored data, bypassing validation.</summary>
    public static Code39 FromStorage(string value) => new(value, false);

    /// <summary>Gets the barcode code string.</summary>
    public string Code => code ?? throw new InvalidOperationException("Code39 not initialized.");

    /// <inheritdoc />
    public bool Equals(Code39 other) => string.Equals(code, other.code, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Code39 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => code is null ? 0 : code.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Code;

    /// <summary>Determines whether two <see cref="Code39"/> instances are equal.</summary>
    public static bool operator ==(Code39 left, Code39 right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="Code39"/> instances are not equal.</summary>
    public static bool operator !=(Code39 left, Code39 right) => !left.Equals(right);

    /// <summary>Explicitly converts a string to a <see cref="Code39"/>.</summary>
    public static explicit operator Code39(string value) => new(value);

    /// <summary>Implicitly converts a <see cref="Code39"/> to its string representation.</summary>
    public static implicit operator string(Code39 barcode) => barcode.Code;
}
