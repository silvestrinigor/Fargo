using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Fargo.Application.Common
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
        /// Gets the first valid page (<c>1</c>).
        /// </summary>
        /// <remarks>
        /// Provides a safe starting page for pagination operations.
        /// </remarks>
        public static Page FirstPage => new(1);

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
                    $"Must be between {MinValue} and {MaxValue}.");
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

        private readonly int value;

        /// <summary>
        /// Gets the page value.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the instance was not properly initialized.
        /// </exception>
        public int Value => value == 0
            ? throw new InvalidOperationException(
                $"{nameof(Page)} was not initialized. Do not use the default value of this struct.")
            : value;

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
}