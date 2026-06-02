using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Fargo.Core.Shared;

/// <summary>
/// Represents a validated textual description in the domain.
///
/// This value object allows empty values, but enforces a maximum length.
/// Because it is implemented as a <see langword="struct"/>, the default
/// uninitialized state is considered invalid and is guarded against when accessed.
/// </summary>
public readonly struct Description :
    IEquatable<Description>,
    IParsable<Description>,
    ISpanParsable<Description>,
    IFormattable,
    ISpanFormattable,
    IUtf8SpanParsable<Description>,
    IUtf8SpanFormattable
{
    /// <summary>
    /// Minimum allowed length for a description.
    /// </summary>
    public const int MinLength = 0;

    /// <summary>
    /// Maximum allowed length for a description.
    /// </summary>
    public const int MaxLength = 500;

    private readonly string? value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Description"/> value object.
    /// </summary>
    /// <param name="value">The textual description.</param>
    public Description(string value)
    {
        Validate(value);
        this.value = value;
    }

    /// <summary>
    /// Gets the underlying string value of the description.
    /// </summary>
    public string Value
        => value ?? throw new InvalidOperationException("Description not initialized.");

    /// <summary>
    /// Gets an empty description.
    /// </summary>
    public static Description Empty => new(string.Empty);

    /// <summary>
    /// Creates a new <see cref="Description"/> from the specified string.
    /// </summary>
    public static Description FromString(string value)
        => new(value);

    /// <inheritdoc />
    public override string ToString()
        => Value;

    /// <inheritdoc />
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            return Value;
        }

        var culture = formatProvider as CultureInfo;

        return format switch
        {
            "U" => Value.ToUpper(culture),
            "L" => Value.ToLower(culture),
            _ => throw new FormatException($"Unsupported format '{format}'.")
        };
    }

    /// <inheritdoc />
    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        string formatted = format.IsEmpty
            ? Value
            : ToString(format.ToString(), provider);

        if (formatted.AsSpan().TryCopyTo(destination))
        {
            charsWritten = formatted.Length;
            return true;
        }

        charsWritten = 0;
        return false;
    }

    /// <inheritdoc />
    public bool TryFormat(
        Span<byte> utf8Destination,
        out int bytesWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        string formatted = format.IsEmpty
            ? Value
            : ToString(format.ToString(), provider);

        return Encoding.UTF8.TryGetBytes(
            formatted,
            utf8Destination,
            out bytesWritten);
    }

    /// <inheritdoc />
    public bool Equals(Description other)
        => string.Equals(value, other.value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is Description other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
        => value is null
            ? 0
            : value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two descriptions are equal.
    /// </summary>
    public static bool operator ==(Description left, Description right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two descriptions are different.
    /// </summary>
    public static bool operator !=(Description left, Description right)
        => !left.Equals(right);

    /// <summary>
    /// Implicitly converts a <see cref="Description"/> to <see cref="string"/>.
    /// </summary>
    public static implicit operator string(Description description)
        => description.Value;

    /// <summary>
    /// Explicitly converts a <see cref="string"/> to <see cref="Description"/>.
    /// </summary>
    public static explicit operator Description(string value)
        => new(value);

    /// <inheritdoc />
    public static Description Parse(string s, IFormatProvider? provider)
        => new(s);

    /// <inheritdoc />
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out Description result)
    {
        try
        {
            result = new Description(s!);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <inheritdoc />
    public static Description Parse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider)
        => new(s.ToString());

    /// <inheritdoc />
    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        out Description result)
    {
        try
        {
            result = new Description(s.ToString());
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <inheritdoc />
    public static Description Parse(
        ReadOnlySpan<byte> utf8Text,
        IFormatProvider? provider)
    {
        string value = Encoding.UTF8.GetString(utf8Text);
        return new Description(value);
    }

    /// <inheritdoc />
    public static bool TryParse(
        ReadOnlySpan<byte> utf8Text,
        IFormatProvider? provider,
        out Description result)
    {
        try
        {
            string value = Encoding.UTF8.GetString(utf8Text);

            result = new Description(value);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Validates the specified description value.
    /// </summary>
    private static void Validate(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Description length must be between {MinLength} and {MaxLength} characters.");
        }
    }
}
