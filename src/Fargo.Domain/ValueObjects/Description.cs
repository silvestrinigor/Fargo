namespace Fargo.Domain.ValueObjects
{
    /// <summary>
    /// Represents a validated textual description in the domain.
    ///
    /// This value object allows empty values, but enforces a maximum length.
    /// Because it is implemented as a <see langword="struct"/>, it also safely
    /// handles the default uninitialized state by exposing an empty string.
    /// </summary>
    public readonly struct Description
    {
        /// <summary>
        /// Minimum allowed length for a description.
        /// </summary>
        public const int MinLength = 0;

        /// <summary>
        /// Maximum allowed length for a description.
        /// </summary>
        public const int MaxLength = 500;

        /// <summary>
        /// Initializes a new empty <see cref="Description"/>.
        /// </summary>
        public Description() : this(string.Empty) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Description"/> value object.
        /// </summary>
        /// <param name="value">The textual description.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the value length is outside the allowed range.
        /// </exception>
        public Description(string value)
        {
            Validate(value);
            this.value = value;
        }

        /// <summary>
        /// Gets the underlying string value of the description.
        ///
        /// When the struct is in its default uninitialized state,
        /// this property returns an empty string.
        /// </summary>
        public string Value => value ?? string.Empty;

        private readonly string? value;

        /// <summary>
        /// Gets an empty description.
        /// </summary>
        public static Description Empty => new(string.Empty);

        /// <summary>
        /// Creates a new <see cref="Description"/> from the specified string.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>A validated <see cref="Description"/> instance.</returns>
        public static Description FromString(string value)
            => new(value);

        /// <summary>
        /// Returns the string representation of the description.
        /// </summary>
        /// <returns>The underlying string value.</returns>
        public override string ToString()
            => Value;

        /// <summary>
        /// Implicitly converts a <see cref="Description"/> to <see cref="string"/>.
        /// </summary>
        /// <param name="description">The description value object.</param>
        public static implicit operator string(Description description)
            => description.Value;

        /// <summary>
        /// Explicitly converts a <see cref="string"/> to <see cref="Description"/>.
        /// </summary>
        /// <param name="value">The string value to convert.</param>
        public static explicit operator Description(string value)
            => new(value);

        /// <summary>
        /// Validates the specified description value.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the value length is outside the allowed range.
        /// </exception>
        private static void Validate(string value)
        {
            ArgumentNullException.ThrowIfNull(value);

            if (value.Length < MinLength || value.Length > MaxLength)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    $"Description length must be between {MinLength} and {MaxLength} characters.");
            }
        }
    }
}