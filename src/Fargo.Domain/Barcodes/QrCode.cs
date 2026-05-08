namespace Fargo.Domain.Barcodes;

/// <summary>QR Code barcode value assigned to an article. Code must be 1–2953 characters.</summary>
public readonly struct QrCode : IEquatable<QrCode>
{
    public const int MinLength = 1;
    public const int MaxLength = 2953;

    private readonly string code;

    /// <exception cref="ArgumentException">Thrown when the value is not 1–2953 characters.</exception>
    public QrCode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length is < MinLength or > MaxLength)
        {
            throw new ArgumentException($"QR Code must be {MinLength}–{MaxLength} characters.", nameof(value));
        }

        code = value;
    }

    private QrCode(string value, bool _) => code = value;

    /// <summary>Reconstructs a <see cref="QrCode"/> from stored data, bypassing validation.</summary>
    public static QrCode FromStorage(string value) => new(value, false);

    /// <summary>Gets the barcode code string.</summary>
    public string Code => code ?? throw new InvalidOperationException("QrCode not initialized.");

    /// <inheritdoc />
    public bool Equals(QrCode other) => string.Equals(code, other.code, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is QrCode other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => code is null ? 0 : code.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc />
    public override string ToString() => Code;

    /// <summary>Determines whether two <see cref="QrCode"/> instances are equal.</summary>
    public static bool operator ==(QrCode left, QrCode right) => left.Equals(right);

    /// <summary>Determines whether two <see cref="QrCode"/> instances are not equal.</summary>
    public static bool operator !=(QrCode left, QrCode right) => !left.Equals(right);

    /// <summary>Explicitly converts a string to a <see cref="QrCode"/>.</summary>
    public static explicit operator QrCode(string value) => new(value);

    /// <summary>Implicitly converts a <see cref="QrCode"/> to its string representation.</summary>
    public static implicit operator string(QrCode barcode) => barcode.Code;
}
