using System.Diagnostics.CodeAnalysis;

namespace Fargo.Core.Informations;

/// <summary>
/// A name that is not blank and has between <see cref="MinLength"/> and <see cref="MaxLength"/> characters.
/// </summary>
public readonly struct Name : IEquatable<Name>, IParsable<Name>
{
    public const int MaxLength = 100;

    public const int MinLength = 1;

    public string Value { get; } = "C";

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

        Value = value;
    }

    public static Name FromString(string value)
        => new(value);

    public override string ToString()
        => Value;

    public bool Equals(Name other)
        => string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj)
        => obj is Name other && Equals(other);

    public override int GetHashCode()
        => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(Name left, Name right)
        => left.Equals(right);

    public static bool operator !=(Name left, Name right)
        => !left.Equals(right);

    public static implicit operator string(Name name)
        => name.Value;

    public static explicit operator Name(string value)
        => new(value);

    public static Name Parse(string s, IFormatProvider? provider)
        => new(s);

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
}
