using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Fargo.Application.Commom
{
    /// <summary>
    /// Represents the page index used in paginated queries.
    /// </summary>
    /// <remarks>
    /// This value object ensures that page numbers remain within
    /// a valid range and supports automatic parsing from strings
    /// using <see cref="IParsable{TSelf}"/>.
    /// </remarks>
    public readonly struct Page
        : IParsable<Page>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Page"/>.
        /// </summary>
        /// <param name="value">The page number.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the page value is outside the allowed range.
        /// </exception>
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

        /// <summary>
        /// Minimum allowed page value.
        /// </summary>
        public const int MinValue = 1;

        /// <summary>
        /// Maximum allowed page value.
        /// </summary>
        public const int MaxValue = int.MaxValue;

        /// <summary>
        /// Default page value used when none is provided.
        /// </summary>
        public const int DefaultValue = MinValue;

        /// <summary>
        /// Gets the effective page value.
        /// </summary>
        /// <remarks>
        /// If the struct is in its default state, the default page value is returned.
        /// </remarks>
        public int Value => value == 0
            ? DefaultValue
            : value;

        private readonly int value;

        /// <summary>
        /// Implicitly converts <see cref="Page"/> to <see cref="int"/>.
        /// </summary>
        public static implicit operator int(Page page)
            => page.Value;

        /// <summary>
        /// Explicitly converts an <see cref="int"/> to <see cref="Page"/>.
        /// </summary>
        public static explicit operator Page(int value)
            => new(value);

        /// <summary>
        /// Parses a string into a <see cref="Page"/>.
        /// </summary>
        public static Page Parse(string s, IFormatProvider? provider)
        {
            if (TryParse(s, provider, out var result))
            {
                return result;
            }

            throw new FormatException($"Invalid Page value: '{s}'.");
        }

        /// <summary>
        /// Attempts to parse a string into a <see cref="Page"/>.
        /// </summary>
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

            if (value < MinValue || value > MaxValue)
            {
                return false;
            }

            result = new Page(value);

            return true;
        }
    }
}