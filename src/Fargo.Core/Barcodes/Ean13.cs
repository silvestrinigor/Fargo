namespace Fargo.Core.Barcodes;

/// <summary>
/// An EAN-13 code: exactly 13 ASCII digits. The check digit is not validated.
/// </summary>
public readonly struct Ean13 : IBarcode, IEquatable<Ean13>
{
    public const int CodeLength = 13;

    public string Value { get; } = "0000000000000";

    public Ean13(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Length != CodeLength || !value.All(char.IsAsciiDigit))
        {
            throw new ArgumentException($"EAN-13 code must be exactly {CodeLength} digits.", nameof(value));
        }

        Value = value;
    }

    public bool Equals(Ean13 other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is Ean13 other && Equals(other);

    public override int GetHashCode() => Value is null ? 0 : Value.GetHashCode(StringComparison.Ordinal);

    public override string ToString() => Value;

    public static bool operator ==(Ean13 left, Ean13 right) => left.Equals(right);

    public static bool operator !=(Ean13 left, Ean13 right) => !left.Equals(right);

    public static explicit operator Ean13(string value) => new(value);

    public static implicit operator string(Ean13 barcode) => barcode.Value;
}
