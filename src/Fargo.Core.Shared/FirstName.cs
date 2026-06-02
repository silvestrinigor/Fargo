using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Fargo.Core.Shared;

/// <summary>
/// Represents a validated first name value object used in the domain.
///
/// This value object guarantees that a first name:
/// - is not null, empty, or whitespace
/// - is within the allowed length range
/// - contains only letters, spaces, or hyphens
/// - does not start or end with spaces or hyphens
/// - does not contain consecutive spaces or hyphens
/// </summary>
public readonly struct FirstName :
    IEquatable<FirstName>,
    IParsable<FirstName>,
    ISpanParsable<FirstName>,
    IFormattable,
    ISpanFormattable,
    IUtf8SpanParsable<FirstName>,
    IUtf8SpanFormattable
{
    /// <summary>
    /// Maximum allowed length for a first name.
    /// </summary>
    public const int MaxLength = 50;

    /// <summary>
    /// Minimum allowed length for a first name.
    /// </summary>
    public const int MinLength = 2;

    private readonly string? value;

    /// <summary>
    /// Initializes a new instance of the <see cref="FirstName"/> struct.
    /// </summary>
    /// <param name="value">The string value representing the first name.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is null, empty, whitespace,
    /// or contains invalid characters or separators.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value length is outside the allowed range.
    /// </exception>
    public FirstName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "First name cannot be null, empty, or whitespace.",
                nameof(value));
        }

        value = value.Trim();

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"First name length must be between {MinLength} and {MaxLength} characters.");
        }

        ValidateCharacters(value);

        this.value = value;
    }

    /// <summary>
    /// Gets the underlying string value of the first name.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the value object was not properly initialized.
    /// This protects against the default struct state.
    /// </exception>
    public string Value
        => value ?? throw new InvalidOperationException("First name not initialized.");

    /// <summary>
    /// Creates a new validated <see cref="FirstName"/> from a string.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <returns>A validated <see cref="FirstName"/> instance.</returns>
    public static FirstName FromString(string value)
        => new(value);

    /// <summary>
    /// Returns the string representation of the first name.
    /// </summary>
    /// <returns>The underlying string value.</returns>
    public override string ToString()
        => Value;

    /// <summary>
    /// Formats the current first name using the specified format string.
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
    /// Formats the current first name into the specified character span.
    /// </summary>
    /// <param name="destination">
    /// The destination buffer to write the formatted value into.
    /// </param>
    /// <param name="charsWritten">
    /// When this method returns, contains the number of characters written.
    /// </param>
    /// <param name="format">The format string.</param>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if formatting succeeded;
    /// otherwise, <see langword="false"/>.
    /// </returns>
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
    /// Formats the current first name into the specified UTF-8 byte span.
    /// </summary>
    /// <param name="utf8Destination">
    /// The destination buffer to write the UTF-8 formatted value into.
    /// </param>
    /// <param name="bytesWritten">
    /// When this method returns, contains the number of bytes written.
    /// </param>
    /// <param name="format">The format string.</param>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if formatting succeeded;
    /// otherwise, <see langword="false"/>.
    /// </returns>
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
    /// Determines whether the current first name is equal to another first name.
    /// </summary>
    /// <param name="other">The other first name to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both first names have the same value;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(FirstName other)
        => string.Equals(value, other.value, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the current first name is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns>
    /// <see langword="true"/> if the specified object is a <see cref="FirstName"/>
    /// with the same value; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is FirstName other && Equals(other);

    /// <summary>
    /// Returns a hash code for the current first name.
    /// </summary>
    /// <returns>A hash code based on the underlying value.</returns>
    public override int GetHashCode()
        => value is null
            ? 0
            : value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two <see cref="FirstName"/> instances are equal.
    /// </summary>
    public static bool operator ==(FirstName left, FirstName right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="FirstName"/> instances are different.
    /// </summary>
    public static bool operator !=(FirstName left, FirstName right)
        => !left.Equals(right);

    /// <summary>
    /// Implicitly converts a <see cref="FirstName"/> to its string representation.
    /// </summary>
    /// <param name="firstName">The first name to convert.</param>
    public static implicit operator string(FirstName firstName)
        => firstName.Value;

    /// <summary>
    /// Explicitly converts a string to a <see cref="FirstName"/>.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    public static explicit operator FirstName(string value)
        => new(value);

    /// <summary>
    /// Parses the specified string into a <see cref="FirstName"/>.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information.
    /// This parameter is ignored.
    /// </param>
    /// <returns>A validated <see cref="FirstName"/> instance.</returns>
    public static FirstName Parse(string s, IFormatProvider? provider)
        => new(s);

    /// <summary>
    /// Attempts to parse the specified string into a <see cref="FirstName"/>.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information.
    /// This parameter is ignored.
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the parsed value if successful;
    /// otherwise, the default value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if parsing succeeded;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out FirstName result)
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

            result = new FirstName(s);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Parses the specified character span into a <see cref="FirstName"/>.
    /// </summary>
    /// <param name="s">The character span to parse.</param>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information.
    /// This parameter is ignored.
    /// </param>
    /// <returns>A validated <see cref="FirstName"/> instance.</returns>
    public static FirstName Parse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider)
        => new(s.ToString());

    /// <summary>
    /// Attempts to parse the specified character span into a <see cref="FirstName"/>.
    /// </summary>
    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        out FirstName result)
        => TryParse(s.ToString(), provider, out result);

    /// <summary>
    /// Parses the specified UTF-8 byte span into a <see cref="FirstName"/>.
    /// </summary>
    /// <param name="utf8Text">The UTF-8 encoded text to parse.</param>
    /// <param name="provider">
    /// An object that supplies culture-specific formatting information.
    /// This parameter is ignored.
    /// </param>
    /// <returns>A validated <see cref="FirstName"/> instance.</returns>
    public static FirstName Parse(
        ReadOnlySpan<byte> utf8Text,
        IFormatProvider? provider)
        => new(Encoding.UTF8.GetString(utf8Text));

    /// <summary>
    /// Attempts to parse the specified UTF-8 byte span into a <see cref="FirstName"/>.
    /// </summary>
    public static bool TryParse(
        ReadOnlySpan<byte> utf8Text,
        IFormatProvider? provider,
        out FirstName result)
    {
        string value = Encoding.UTF8.GetString(utf8Text);

        return TryParse(value, provider, out result);
    }

    /// <summary>
    /// Validates the characters and separator rules of a first name.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when invalid characters or separator sequences are found.
    /// </exception>
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
                        "First name cannot start or end with a separator, or contain consecutive separators.",
                        nameof(value));
                }

                previousWasSeparator = true;
                continue;
            }

            throw new ArgumentException(
                "First name can contain only letters, spaces, or hyphens.",
                nameof(value));
        }
    }
}
