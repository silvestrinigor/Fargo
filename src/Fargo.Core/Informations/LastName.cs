using System.Diagnostics.CodeAnalysis;

namespace Fargo.Core.Informations;

/// <summary>
/// A last name of 1 to 100 characters, trimmed. It may contain only letters, spaces and hyphens,
/// and cannot start or end with a separator or contain consecutive separators.
/// </summary>
public readonly struct LastName :
    IEquatable<LastName>,
    IParsable<LastName>
{
    public const int MaxLength = 100;

    public const int MinLength = 1;

    public string Value { get; } = "B";

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

        Value = value;
    }

    public static LastName FromString(string value)
        => new(value);

    public override string ToString()
        => Value;

    public bool Equals(LastName other)
        => string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj)
        => obj is LastName other && Equals(other);

    public override int GetHashCode()
        => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(LastName left, LastName right)
        => left.Equals(right);

    public static bool operator !=(LastName left, LastName right)
        => !left.Equals(right);

    public static implicit operator string(LastName lastName)
        => lastName.Value;

    public static explicit operator LastName(string value)
        => new(value);

    public static LastName Parse(string s, IFormatProvider? provider)
        => new(s);

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
