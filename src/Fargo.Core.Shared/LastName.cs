using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Fargo.Core.Shared;

/// <summary>
/// Represents a validated last name value object used in the domain.
///
/// This value object guarantees that a last name:
/// - is not null, empty, or whitespace
/// - is within the allowed length range
/// - contains only letters, spaces, or hyphens
/// - does not start or end with separators
/// - does not contain consecutive separators
/// </summary>
public readonly struct LastName :
    IEquatable<LastName>,
    IParsable<LastName>,
    ISpanParsable<LastName>,
    IFormattable,
    ISpanFormattable,
    IUtf8SpanParsable<LastName>,
    IUtf8SpanFormattable
{
    /// <summary>
    /// Maximum allowed length for a last name.
    /// </summary>
    public const int MaxLength = 100;

    /// <summary>
    /// Minimum allowed length for a last name.
    /// </summary>
    public const int MinLength = 2;

    private readonly string? value;

    /// <summary>
    /// Initializes a new instance of the <see cref="LastName"/> struct.
    /// </summary>
    /// <param name="value">The string value representing the last name.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is null, empty, whitespace,
    /// or contains invalid characters or separators.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value length is outside the allowed range.
    /// </exception>
    public LastName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Last name cannot be null, empty, or whitespace.",
                nameof(value));
        }

        value = value.Trim();

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Last name length must be between {MinLength} and {MaxLength} characters.");
        }

        ValidateCharacters(value);

        this.value = value;
    }

    /// <summary>
    /// Gets the underlying string value of the last name.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the value object was not properly initialized.
    /// This protects against the default struct state.
    /// </exception>
    public string Value
        => value ?? throw new InvalidOperationException("Last name not initialized.");

    /// <summary>
    /// Creates a new validated <see cref="LastName"/> from a string.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <returns>A validated <see cref="LastName"/> instance.</returns>
    public static LastName FromString(string value)
        => new(value);

    /// <summary>
    /// Returns the string representation of the last name.
    /// </summary>
    /// <returns>The underlying string value.</returns>
    public override string ToString()
        => Value;

    /// <summary>
    /// Formats the current last name using the specified format string.
    /// </summary>
    /// <param name="format">
    /// The format string to use.
    /// Supported formats:
    /// <list type="bullet">
    /// <item><description><c>G</c> or <see langword="null"/> - original value</description></item>
    /// <item><description><c>U</c> - uppercase</description></item>
    /// <item><description><c>L</c> - lowercase</description></item>
    /// </list>
    /// </param>
    /// <param name="formatProvider">
    /// An object that supplies culture-specific formatting information.
    /// </param>
    /// <returns>The formatted string representation.</returns>
    /// <exception cref="FormatException">
    /// Thrown when the format string is not supported.
    /// </exception>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        format ??= "G";

        return format switch
        {
            "G" => Value,
            "U" => Value.ToUpper(formatProvider as CultureInfo),
            "L" => Value.ToLower(formatProvider as CultureInfo),
            _ => throw new FormatException($"Unsupported format '{format}'.")
        };
    }

    /// <summary>
    /// Formats the current last name into the specified character span.
    /// </summary>
    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        string formatted = ToString(
            format.IsEmpty ? "G" : format.ToString(),
            provider);

        if (formatted.AsSpan().TryCopyTo(destination))
        {
            charsWritten = formatted.Length;
            return true;
        }

        charsWritten = 0;
        return false;
    }

    /// <summary>
    /// Formats the current last name into the specified UTF-8 byte span.
    /// </summary>
    public bool TryFormat(
        Span<byte> utf8Destination,
        out int bytesWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        string formatted = ToString(
            format.IsEmpty ? "G" : format.ToString(),
            provider);

        return Encoding.UTF8.TryGetBytes(
            formatted,
            utf8Destination,
            out bytesWritten);
    }

    /// <summary>
    /// Determines whether the current last name is equal to another last name.
    /// </summary>
    public bool Equals(LastName other)
        => string.Equals(value, other.value, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the current last name is equal to the specified object.
    /// </summary>
    public override bool Equals(object? obj)
        => obj is LastName other && Equals(other);

    /// <summary>
    /// Returns a hash code for the current last name.
    /// </summary>
    public override int GetHashCode()
        => value is null
            ? 0
            : value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two <see cref="LastName"/> instances are equal.
    /// </summary>
    public static bool operator ==(LastName left, LastName right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="LastName"/> instances are different.
    /// </summary>
    public static bool operator !=(LastName left, LastName right)
        => !left.Equals(right);

    /// <summary>
    /// Implicitly converts a <see cref="LastName"/> to its string representation.
    /// </summary>
    public static implicit operator string(LastName lastName)
        => lastName.Value;

    /// <summary>
    /// Explicitly converts a string to a <see cref="LastName"/>.
    /// </summary>
    public static explicit operator LastName(string value)
        => new(value);

    /// <summary>
    /// Parses the specified string into a <see cref="LastName"/>.
    /// </summary>
    public static LastName Parse(string s, IFormatProvider? provider)
        => new(s);

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="LastName"/>.
    /// </summary>
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out LastName result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        s = s.Trim();

        if (s.Length < MinLength || s.Length > MaxLength)
        {
            return false;
        }

        try
        {
            ValidateCharacters(s);

            result = new LastName(s);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Parses the specified character span into a <see cref="LastName"/>.
    /// </summary>
    public static LastName Parse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider)
        => new(s.ToString());

    /// <summary>
    /// Attempts to parse the specified character span into a <see cref="LastName"/>.
    /// </summary>
    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        out LastName result)
        => TryParse(s.ToString(), provider, out result);

    /// <summary>
    /// Parses the specified UTF-8 byte span into a <see cref="LastName"/>.
    /// </summary>
    public static LastName Parse(
        ReadOnlySpan<byte> utf8Text,
        IFormatProvider? provider)
        => new(Encoding.UTF8.GetString(utf8Text));

    /// <summary>
    /// Attempts to parse the specified UTF-8 byte span into a <see cref="LastName"/>.
    /// </summary>
    public static bool TryParse(
        ReadOnlySpan<byte> utf8Text,
        IFormatProvider? provider,
        out LastName result)
    {
        string value = Encoding.UTF8.GetString(utf8Text);

        return TryParse(value, provider, out result);
    }

    private static void ValidateCharacters(string value)
    {
        var previousWasSeparator = false;

        for (var i = 0; i < value.Length; i++)
        {
            var current = value[i];
            var isSeparator = current == ' ' || current == '-';

            if (char.IsLetter(current))
            {
                previousWasSeparator = false;
                continue;
            }

            if (isSeparator)
            {
                if (i == 0 || i == value.Length - 1 || previousWasSeparator)
                {
                    throw new ArgumentException(
                        "Last name cannot start or end with a separator, or contain consecutive separators.",
                        nameof(value));
                }

                previousWasSeparator = true;
                continue;
            }

            throw new ArgumentException(
                "Last name can contain only letters, spaces, or hyphens.",
                nameof(value));
        }
    }
}
