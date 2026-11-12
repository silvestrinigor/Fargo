using System.Diagnostics.CodeAnalysis;

namespace Fargo.Core.Informations;

/// <summary>
/// A first name of 1 to 50 characters, trimmed. It may contain only letters, spaces and hyphens,
/// and cannot start or end with a separator or contain consecutive separators.
/// </summary>
public readonly struct FirstName : IEquatable<FirstName>, IParsable<FirstName>
{
    public const int MaxLength = 50;

    public const int MinLength = 1;

    public string Value { get; } = "A";

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

        Value = value;
    }

    public static FirstName FromString(string value)
        => new(value);

    public override string ToString()
        => Value;

    public bool Equals(FirstName other)
        => string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj)
        => obj is FirstName other && Equals(other);

    public override int GetHashCode()
        => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(FirstName left, FirstName right)
        => left.Equals(right);

    public static bool operator !=(FirstName left, FirstName right)
        => !left.Equals(right);

    public static implicit operator string(FirstName firstName)
        => firstName.Value;

    public static explicit operator FirstName(string value)
        => new(value);

    public static FirstName Parse(string s, IFormatProvider? provider)
        => new(s);

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
