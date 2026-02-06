using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Fargo.Application.Commom
{
    public readonly struct Page
        : IParsable<Page>
    {
        public Page(int value)
        {
            if (value < MinValue || value > MaxValue)
            {
                throw new ArgumentOutOfRangeException(
                        nameof(value),
                        $"Must be between {MinValue} and {MaxValue}"
                        );
            }

            this.value = value;
        }

        public const int MinValue = 1;

        public const int MaxValue = int.MaxValue;

        public const int DefaultValue = MinValue;

        public int Value => value == 0
            ? DefaultValue
            : value;

        private readonly int value;

        public static implicit operator int(Page page)
            => page.Value;

        public static explicit operator Page(int value)
            => new(value);

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
                [MaybeNullWhen(false)] out Page result
                )
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
                    out var value
                    );

            if (!parsed)
            {
                return false;
            }

            if (value < MinValue)
            {
                return false;
            }

            result = new Page(value);

            return true;
        }
    }
}