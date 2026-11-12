using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Fargo.Application.Common.Paging;

public readonly struct Page : IParsable<Page>
{
    public const int MinValue = 1;

    public const int MaxValue = int.MaxValue;

    public int Value { get; }

    public static Page FirstPage => new(1);

    public Page(int value)
    {
        if (value < MinValue || value > MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                $"Must be between {MinValue} and {MaxValue}.");
        }

        Value = value;
    }

    public Page()
    {
        Value = 1;
    }

    public static implicit operator int(Page page) => page.Value;

    public static explicit operator Page(int value) => new(value);

    public static Page Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out var result))
        {
            return result;
        }

        throw new FormatException($"Invalid Page value: '{s}'.");
    }

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out Page result)
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

        result = new Page(value);
        return true;
    }
}
