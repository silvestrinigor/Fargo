namespace Fargo.Domain.Barcodes;

/// <summary>Data Matrix barcode value assigned to an article. Code must be 1–2335 characters.</summary>
public readonly struct DataMatrix : IEquatable<DataMatrix>
{
    public const int MinLength = 1;
    public const int MaxLength = 2335;

    private readonly string code;

    /// <exception cref="ArgumentException">Thrown when the value is not 1–2335 characters.</exception>
    public DataMatrix(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length is < MinLength or > MaxLength)
            throw new ArgumentException($"Data Matrix code must be {MinLength}–{MaxLength} characters.", nameof(value));
        code = value;
    }

    private DataMatrix(string value, bool _) => code = value;

    /// <summary>Reconstructs a <see cref="DataMatrix"/> from stored data, bypassing validation.</summary>
    public static DataMatrix FromStorage(string value) => new(value, false);

    /// <summary>Gets the barcode code string.</summary>
    public string Code => code ?? throw new InvalidOperationException("DataMatrix not initialized.");

    /// <inheritdoc />
    public bool Equals(DataMatrix other) => string.Equals(code, other.code, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is DataMatrix other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => code is null ? 0 : code.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Code;

    /// <summary>Determines whether two <see cref="DataMatrix"/> instances are equal.</summary>
    public static bool operator ==(DataMatrix left, DataMatrix right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="DataMatrix"/> instances are not equal.</summary>
    public static bool operator !=(DataMatrix left, DataMatrix right) => !left.Equals(right);

    /// <summary>Explicitly converts a string to a <see cref="DataMatrix"/>.</summary>
    public static explicit operator DataMatrix(string value) => new(value);

    /// <summary>Implicitly converts a <see cref="DataMatrix"/> to its string representation.</summary>
    public static implicit operator string(DataMatrix barcode) => barcode.Code;
}
