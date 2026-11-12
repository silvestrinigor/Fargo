using System.Diagnostics.CodeAnalysis;

namespace Fargo.Core.Informations;

/// <summary>
/// A free text of at most <see cref="MaxLength"/> characters; unlike <see cref="Name"/>, it may be empty.
/// </summary>
public readonly struct Description : IEquatable<Description>, IParsable<Description>
{
    public const int MaxLength = 500;

    public string Value { get; } = string.Empty;

    public Description(string value)
    {
        Validate(value);
        Value = value;
    }

    public static Description Empty => new(string.Empty);

    public static Description FromString(string value)
        => new(value);

    public override string ToString()
        => Value;

    public bool Equals(Description other)
        => string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj)
        => obj is Description other && Equals(other);

    public override int GetHashCode()
        => Value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(Description left, Description right)
        => left.Equals(right);

    public static bool operator !=(Description left, Description right)
        => !left.Equals(right);

    public static implicit operator string(Description description)
        => description.Value;

    public static explicit operator Description(string value)
        => new(value);

    public static Description Parse(string s, IFormatProvider? provider)
        => new(s);

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out Description result)
    {
        try
        {
            result = new Description(s!);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    private static void Validate(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Description length must be shorter or equal to {MaxLength} characters.");
        }
    }
}
