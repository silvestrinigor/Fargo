using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Fargo.Core.Shared;

/// <summary>
/// Represents a validated first name.
/// </summary>
public readonly struct FirstName : IEquatable<FirstName>, IParsable<FirstName>, ISpanParsable<FirstName>, IFormattable, ISpanFormattable, IUtf8SpanParsable<FirstName>, IUtf8SpanFormattable
{
    /// <summary>
    /// Maximum allowed length for a first name.
    /// </summary>
    public const int MaxLength = 50;

    /// <summary>
    /// Minimum allowed length for a first name.
    /// </summary>
    public const int MinLength = 2;

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

    private readonly string? value;

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the value object was not properly initialized.
    /// This protects against the default struct state.
    /// </exception>
    public string Value
        => value ?? throw new InvalidOperationException("First name not initialized.");

    /// <summary>
    /// Creates a first name from a string.
    /// </summary>
    public static FirstName FromString(string value)
        => new(value);

    /// <inheritdoc />
    public override string ToString()
        => Value;

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public bool Equals(FirstName other)
        => string.Equals(value, other.value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is FirstName other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
        => value is null
            ? 0
            : value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two first names are equal.
    /// </summary>
    public static bool operator ==(FirstName left, FirstName right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two first names are different.
    /// </summary>
    public static bool operator !=(FirstName left, FirstName right)
        => !left.Equals(right);

    /// <summary>
    /// Converts a first name to a string.
    /// </summary>
    public static implicit operator string(FirstName firstName)
        => firstName.Value;

    /// <summary>
    /// Converts a string to a first name.
    /// </summary>
    public static explicit operator FirstName(string value)
        => new(value);

    /// <inheritdoc />
    public static FirstName Parse(string s, IFormatProvider? provider)
        => new(s);

    /// <inheritdoc />
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

    /// <inheritdoc />
    public static FirstName Parse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider)
        => new(s.ToString());

    /// <inheritdoc />
    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        out FirstName result)
        => TryParse(s.ToString(), provider, out result);

    /// <inheritdoc />
    public static FirstName Parse(
        ReadOnlySpan<byte> utf8Text,
        IFormatProvider? provider)
        => new(Encoding.UTF8.GetString(utf8Text));

    /// <inheritdoc />
    public static bool TryParse(
        ReadOnlySpan<byte> utf8Text,
        IFormatProvider? provider,
        out FirstName result)
    {
        string value = Encoding.UTF8.GetString(utf8Text);

        return TryParse(value, provider, out result);
    }

    /// <summary>
    /// Validates the value.
    /// </summary>
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
