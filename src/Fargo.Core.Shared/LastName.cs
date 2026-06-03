using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Fargo.Core.Shared;

/// <summary>
/// Represents a validated last name.
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
    /// Maximum length.
    /// </summary>
    public const int MaxLength = 100;

    /// <summary>
    /// Minimum length.
    /// </summary>
    public const int MinLength = 2;

    private readonly string? value;

    /// <summary>
    /// Initializes a last name.
    /// </summary>
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
    /// Gets the value.
    /// </summary>
    public string Value
        => value ?? throw new InvalidOperationException("Last name not initialized.");

    /// <summary>
    /// Creates a last name from a string.
    /// </summary>
    public static LastName FromString(string value)
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
    public bool Equals(LastName other)
        => string.Equals(value, other.value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is LastName other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
        => value is null
            ? 0
            : value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two last names are equal.
    /// </summary>
    public static bool operator ==(LastName left, LastName right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two last names are different.
    /// </summary>
    public static bool operator !=(LastName left, LastName right)
        => !left.Equals(right);

    /// <summary>
    /// Converts a last name to a string.
    /// </summary>
    public static implicit operator string(LastName lastName)
        => lastName.Value;

    /// <summary>
    /// Converts a string to a last name.
    /// </summary>
    public static explicit operator LastName(string value)
        => new(value);

    /// <inheritdoc />
    public static LastName Parse(string s, IFormatProvider? provider)
        => new(s);

    /// <inheritdoc />
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

    /// <inheritdoc />
    public static LastName Parse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider)
        => new(s.ToString());

    /// <inheritdoc />
    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        out LastName result)
        => TryParse(s.ToString(), provider, out result);

    /// <inheritdoc />
    public static LastName Parse(
        ReadOnlySpan<byte> utf8Text,
        IFormatProvider? provider)
        => new(Encoding.UTF8.GetString(utf8Text));

    /// <inheritdoc />
    public static bool TryParse(
        ReadOnlySpan<byte> utf8Text,
        IFormatProvider? provider,
        out LastName result)
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
