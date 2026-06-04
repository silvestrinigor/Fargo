using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Fargo.Core.Shared;

/// <summary>
/// Represents a validated nameid.
/// </summary>
public readonly struct Nameid :
    IEquatable<Nameid>,
    IParsable<Nameid>,
    ISpanParsable<Nameid>,
    IFormattable,
    ISpanFormattable,
    IUtf8SpanParsable<Nameid>,
    IUtf8SpanFormattable
{
    /// <summary>
    /// Maximum length.
    /// </summary>
    public const int MaxLength = 100;

    /// <summary>
    /// Minimum length.
    /// </summary>
    public const int MinLength = 3;

    private readonly string? value;

    /// <summary>
    /// Initializes a nameid.
    /// </summary>
    public Nameid(string value)
    {
        Validate(value);
        this.value = value.ToLowerInvariant();
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    public string Value
        => value ?? throw new InvalidOperationException("Nameid not initialized.");

    /// <summary>
    /// Creates a nameid from a string.
    /// </summary>
    public static Nameid FromString(string value)
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
    public bool Equals(Nameid other)
        => string.Equals(value, other.value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is Nameid other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
        => value is null
            ? 0
            : StringComparer.OrdinalIgnoreCase.GetHashCode(value);

    /// <summary>
    /// Determines whether two nameids are equal.
    /// </summary>
    public static bool operator ==(Nameid left, Nameid right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two nameids are different.
    /// </summary>
    public static bool operator !=(Nameid left, Nameid right)
        => !left.Equals(right);

    /// <summary>
    /// Converts a nameid to a string.
    /// </summary>
    public static implicit operator string(Nameid nameid)
        => nameid.Value;

    /// <summary>
    /// Converts a string to a nameid.
    /// </summary>
    public static explicit operator Nameid(string value)
        => new(value);

    /// <inheritdoc />
    public static Nameid Parse(string s, IFormatProvider? provider)
        => new(s);

    /// <inheritdoc />
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out Nameid result)
    {
        try
        {
            result = new Nameid(s!);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <inheritdoc />
    public static Nameid Parse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider)
        => new(s.ToString());

    /// <inheritdoc />
    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        out Nameid result)
    {
        try
        {
            result = new Nameid(s.ToString());
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <inheritdoc />
    public static Nameid Parse(
        ReadOnlySpan<byte> utf8Text,
        IFormatProvider? provider)
    {
        string value = Encoding.UTF8.GetString(utf8Text);
        return new Nameid(value);
    }

    /// <inheritdoc />
    public static bool TryParse(
        ReadOnlySpan<byte> utf8Text,
        IFormatProvider? provider,
        out Nameid result)
    {
        try
        {
            string value = Encoding.UTF8.GetString(utf8Text);

            result = new Nameid(value);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Validates the value.
    /// </summary>
    private static void Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Nameid cannot be null, empty, or whitespace.",
                nameof(value));
        }

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Nameid length must be between {MinLength} and {MaxLength} characters.");
        }

        if (value != value.Trim())
        {
            throw new ArgumentException(
                "Nameid cannot start or end with whitespace.",
                nameof(value));
        }

        if (value.Contains(' '))
        {
            throw new ArgumentException(
                "Nameid cannot contain spaces.",
                nameof(value));
        }

        if (!char.IsLetterOrDigit(value[0]))
        {
            throw new ArgumentException(
                "Nameid must start with a letter or digit.",
                nameof(value));
        }

        if (!char.IsLetterOrDigit(value[^1]))
        {
            throw new ArgumentException(
                "Nameid must end with a letter or digit.",
                nameof(value));
        }

        for (var i = 0; i < value.Length; i++)
        {
            var current = value[i];
            var isAllowed =
                char.IsLetterOrDigit(current) ||
                current == '.' ||
                current == '_' ||
                current == '-';

            if (!isAllowed)
            {
                throw new ArgumentException(
                    "Nameid can only contain letters, digits, '.', '_' and '-'.",
                    nameof(value));
            }

            if (i > 0)
            {
                var previous = value[i - 1];
                var currentIsSeparator = current == '.' || current == '_' || current == '-';
                var previousIsSeparator = previous == '.' || previous == '_' || previous == '-';

                if (currentIsSeparator && previousIsSeparator)
                {
                    throw new ArgumentException(
                        "Nameid cannot contain consecutive special characters.",
                        nameof(value));
                }
            }
        }
    }
}
