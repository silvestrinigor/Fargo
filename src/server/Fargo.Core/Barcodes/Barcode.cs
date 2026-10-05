namespace Fargo.Core.Barcodes;

/// <summary>
/// Represents a barcode with its data and format.
/// </summary>
public readonly struct Barcode : IParsable<Barcode>, IEquatable<Barcode>
{
    /// <summary>
    /// Gets the barcode data.
    /// </summary>
    public IBarcode Value { get; }

    /// <summary>
    /// Gets the format of the barcode.
    /// </summary>
    public BarcodeFormat BarcodeFormat { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Barcode"/> struct.
    /// </summary>
    /// <param name="value">The barcode data. Cannot be null.</param>
    /// <param name="format">The format of the barcode.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public Barcode(IBarcode value, BarcodeFormat format)
    {
        ArgumentNullException.ThrowIfNull(value);

        Value = value;
        BarcodeFormat = format;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Barcode"/> struct with default values.
    /// </summary>
    public Barcode()
    {
        Value = new BarcodeNone();
        BarcodeFormat = BarcodeFormat.None;
    }

    /// <summary>
    /// Returns a string representation of the barcode in the format "Value:Format".
    /// </summary>
    /// <returns>A string representation of the barcode.</returns>
    public override string ToString() => $"{Value}:{BarcodeFormat.ToString().ToLowerInvariant()}";

    /// <summary>
    /// Parses a string representation of a barcode into a <see cref="Barcode"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">The format provider. Not used in this implementation.</param>
    /// <returns>A <see cref="Barcode"/> instance parsed from the string.</returns>
    /// <exception cref="FormatException">Thrown when the string is not a valid barcode representation.</exception>
    public static Barcode Parse(string s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, out var result))
        {
            throw new FormatException($"Invalid barcode value: '{s}'.");
        }

        return result;
    }

    /// <summary>
    /// Attempts to parse a string representation of a barcode into a <see cref="Barcode"/> instance.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">The format provider. Not used in this implementation.</param>
    /// <param name="result">When this method returns, contains the parsed barcode, or the default value if parsing failed.</param>
    /// <returns><c>true</c> if parsing was successful; otherwise, <c>false</c>.</returns>
    public static bool TryParse(string? s, IFormatProvider? provider, out Barcode result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        var separator = s.LastIndexOf(':');
        if (separator <= 0 || separator == s.Length - 1)
        {
            return false;
        }

        var value = s[..separator];
        var formatText = s[(separator + 1)..];

        if (!Enum.TryParse<BarcodeFormat>(formatText, true, out var format))
        {
            return false;
        }

        if (!BarcodeFactory.TryCreate(format, value, out var barcode))
        {
            return false;
        }

        result = new Barcode(barcode, format);
        return true;
    }

    /// <summary>
    /// Determines whether the specified <see cref="Barcode"/> is equal to the current <see cref="Barcode"/>.
    /// </summary>
    /// <param name="other">The <see cref="Barcode"/> to compare with the current <see cref="Barcode"/>.</param>
    /// <returns><c>true</c> if the specified <see cref="Barcode"/> is equal to the current <see cref="Barcode"/>; otherwise, <c>false</c>.</returns>
    public bool Equals(Barcode other)
        => Value.Equals(other.Value) && BarcodeFormat.Equals(other.BarcodeFormat);

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="Barcode"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current <see cref="Barcode"/>.</param>
    /// <returns><c>true</c> if the specified object is equal to the current <see cref="Barcode"/>; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
        => obj is Barcode other && Equals(other);

    /// <summary>
    /// Serves as a hash function for the <see cref="Barcode"/>.
    /// </summary>
    /// <returns>A hash code for the current <see cref="Barcode"/>.</returns>
    public override int GetHashCode()
    {
        // Include both Value and BarcodeFormat in hash calculation to avoid collisions
        return HashCode.Combine(Value, BarcodeFormat);
    }

    /// <summary>
    /// Determines whether two specified <see cref="Barcode"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="Barcode"/> to compare.</param>
    /// <param name="right">The second <see cref="Barcode"/> to compare.</param>
    /// <returns><c>true</c> if the two <see cref="Barcode"/> instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(Barcode left, Barcode right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two specified <see cref="Barcode"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="Barcode"/> to compare.</param>
    /// <param name="right">The second <see cref="Barcode"/> to compare.</param>
    /// <returns><c>true</c> if the two <see cref="Barcode"/> instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Barcode left, Barcode right)
        => !left.Equals(right);
}
