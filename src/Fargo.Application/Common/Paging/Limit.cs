using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Fargo.Application.Common.Paging;

/// <remarks>
/// A limit built through the constructor is within <see cref="MinValue"/> and <see cref="MaxValue"/>;
/// the default value is <see cref="int.MaxValue"/>, meaning no practical limit.
/// </remarks>
public readonly struct Limit : IParsable<Limit>
{
    public const int MinValue = 1;

    public const int MaxValue = 1000;

    public int Value { get; }

    public static Limit MaxLimit => new(MaxValue);

    public Limit(int value)
    {
        if (value < MinValue || value > MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                $"Must be between {MinValue} and {MaxValue}.");
        }

        Value = value;
    }

    public Limit()
    {
        Value = int.MaxValue;
    }

    public static implicit operator int(Limit limit) => limit.Value;

    public static explicit operator Limit(int value) => new(value);

    public static Limit Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out var result))
        {
            return result;
        }

        throw new FormatException($"Invalid Limit value: '{s}'.");
    }

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out Limit result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        var parsed = int.TryParse(
            s,
            NumberStyles.Integer,
            provider ?? CultureInfo.InvariantCulture,
            out var value);

        if (!parsed)
        {
            return false;
        }

        if (value < MinValue || value > MaxValue)
        {
            return false;
        }

        result = new Limit(value);
        return true;
    }
}
