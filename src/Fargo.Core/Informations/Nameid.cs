using System.Diagnostics.CodeAnalysis;

namespace Fargo.Core.Informations;

/// <summary>
/// Represents a nameid.
/// </summary>
public readonly struct Nameid :
    IEquatable<Nameid>,
    IParsable<Nameid>
{
    /// <summary>
    /// Maximum length.
    /// </summary>
    public const int MaxLength = 100;

    /// <summary>
    /// Minimum length.
    /// </summary>
    public const int MinLength = 3;

    /// <summary>
    /// Gets the value.
    /// </summary>
    public string Value { get; } = "aaa";

    /// <summary>
    /// Initializes a nameid.
    /// </summary>
    public Nameid(string value)
    {
        Validate(value);
        Value = value.ToLowerInvariant();
    }

    /// <summary>
    /// Creates a nameid from a string.
    /// </summary>
    public static Nameid FromString(string value)
        => new(value);

    /// <inheritdoc />
    public override string ToString()
        => Value;

    /// <inheritdoc />
    public bool Equals(Nameid other)
        => string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is Nameid other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
        => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

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
