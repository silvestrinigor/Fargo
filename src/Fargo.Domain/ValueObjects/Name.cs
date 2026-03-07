namespace Fargo.Domain.ValueObjects
{
    /// <summary>
    /// Represents a validated name value object used across the domain.
    ///
    /// This value object guarantees that a name is always within the
    /// allowed length range and is not null, empty, or composed only of whitespace.
    /// </summary>
    public readonly struct Name
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Name"/> struct.
        /// </summary>
        /// <param name="value">The string value representing the name.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the value is null, empty, or contains only whitespace.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the value length is outside the allowed range.
        /// </exception>
        public Name(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name cannot be null, empty, or whitespace.", nameof(value));

            if (value.Length < MinLength || value.Length > MaxLength)
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    $"Name length must be between {MinLength} and {MaxLength} characters."
                );

            this.value = value;
        }

        /// <summary>
        /// Maximum allowed length for a name.
        /// </summary>
        public const int MaxLength = 100;

        /// <summary>
        /// Minimum allowed length for a name.
        /// </summary>
        public const int MinLength = 3;

        /// <summary>
        /// Gets the underlying string value of the name.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the value object was not properly initialized.
        /// </exception>
        public string Value
            => value != string.Empty
                ? value
                : throw new InvalidOperationException("Name not initialized.");

        private readonly string value;

        /// <summary>
        /// Creates a new <see cref="Name"/> from a string.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>A validated <see cref="Name"/> instance.</returns>
        public static Name FromString(string value)
            => new(value);

        /// <summary>
        /// Implicitly converts a <see cref="Name"/> to its string representation.
        /// </summary>
        public static implicit operator string(Name name) => name.Value;

        /// <summary>
        /// Explicitly converts a string to a <see cref="Name"/>.
        /// </summary>
        public static explicit operator Name(string value) => new(value);

        /// <summary>
        /// Creates a new validated <see cref="Name"/>.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <returns>A new <see cref="Name"/> instance.</returns>
        public static Name NewName(string value)
            => new(value);
    }
}