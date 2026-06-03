using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Fargo.Core.Shared;

/// <summary>
/// Represents a validated name.
/// </summary>
public readonly struct Name : IEquatable<Name>, IParsable<Name>, ISpanParsable<Name>, IFormattable, ISpanFormattable, IUtf8SpanParsable<Name>, IUtf8SpanFormattable
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
    /// Initializes a name.
    /// </summary>
    public Name(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Name cannot be null, empty, or whitespace.",
                nameof(value));
        }

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Name length must be between {MinLength} and {MaxLength} characters.");
        }

        this.value = value;
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    public string Value
        => value ?? throw new InvalidOperationException("Name not initialized.");

    /// <summary>
    /// Creates a name from a string.
    /// </summary>
    public static Name FromString(string value)
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
    public bool Equals(Name other)
        => string.Equals(value, other.value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is Name other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
        => value is null
            ? 0
            : value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two names are equal.
    /// </summary>
    public static bool operator ==(Name left, Name right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two names are different.
    /// </summary>
    public static bool operator !=(Name left, Name right)
        => !left.Equals(right);

    /// <summary>
    /// Converts a name to a string.
    /// </summary>
    public static implicit operator string(Name name)
        => name.Value;

    /// <summary>
    /// Converts a string to a name.
    /// </summary>
    public static explicit operator Name(string value)
        => new(value);

    /// <inheritdoc />
    public static Name Parse(string s, IFormatProvider? provider)
        => new(s);

    /// <inheritdoc />
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out Name result)
    {
        try
        {
            result = new Name(s!);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <inheritdoc />
    public static Name Parse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider)
        => new(s.ToString());

    /// <inheritdoc />
    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        out Name result)
    {
        try
        {
            result = new Name(s.ToString());
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <inheritdoc />
    public static Name Parse(
        ReadOnlySpan<byte> utf8Text,
        IFormatProvider? provider)
    {
        string value = Encoding.UTF8.GetString(utf8Text);
        return new Name(value);
    }

    /// <inheritdoc />
    public static bool TryParse(
        ReadOnlySpan<byte> utf8Text,
        IFormatProvider? provider,
        out Name result)
    {
        try
        {
            string value = Encoding.UTF8.GetString(utf8Text);

            result = new Name(value);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }
}
