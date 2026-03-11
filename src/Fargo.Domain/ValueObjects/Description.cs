namespace Fargo.Domain.ValueObjects
{
    /// <summary>
    /// Represents a validated textual description in the domain.
    ///
    /// This value object allows empty values, but enforces a maximum length.
    /// Because it is implemented as a <see langword="struct"/>, the default
    /// uninitialized state is considered invalid and is guarded against when accessed.
    /// </summary>
    public readonly struct Description : IEquatable<Description>
    {
        /// <summary>
        /// Minimum allowed length for a description.
        /// </summary>
        public const int MinLength = 0;

        /// <summary>
        /// Maximum allowed length for a description.
        /// </summary>
        public const int MaxLength = 500;

        private readonly string? value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Description"/> value object.
        /// </summary>
        /// <param name="value">The textual description.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the description length is outside the allowed range.
        /// </exception>
        public Description(string value)
        {
            Validate(value);
            this.value = value;
        }

        /// <summary>
        /// Gets the underlying string value of the description.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the value object was not properly initialized.
        /// This protects against the default struct state.
        /// </exception>
        public string Value
            => value ?? throw new InvalidOperationException("Description not initialized.");

        /// <summary>
        /// Gets an empty description.
        /// </summary>
        public static Description Empty => new(string.Empty);

        /// <summary>
        /// Creates a new <see cref="Description"/> from the specified string.
        /// </summary>
        /// <param name="value">The textual description.</param>
        /// <returns>A validated <see cref="Description"/> instance.</returns>
        public static Description FromString(string value)
            => new(value);

        /// <summary>
        /// Returns the string representation of the description.
        /// </summary>
        public override string ToString()
            => Value;

        /// <summary>
        /// Determines whether the current description is equal to another.
        /// </summary>
        /// <param name="other">The description to compare with the current instance.</param>
        /// <returns>
        /// <see langword="true"/> if both descriptions are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(Description other)
            => string.Equals(value, other.value, StringComparison.Ordinal);

        /// <summary>
        /// Determines whether the current description is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>
        /// <see langword="true"/> if the specified object is a <see cref="Description"/>
        /// and is equal to the current instance; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object? obj)
            => obj is Description other && Equals(other);

        /// <summary>
        /// Returns a hash code for the description.
        /// </summary>
        public override int GetHashCode()
            => value is null ? 0 : value.GetHashCode(StringComparison.Ordinal);

        /// <summary>
        /// Determines whether two descriptions are equal.
        /// </summary>
        /// <param name="left">The first description to compare.</param>
        /// <param name="right">The second description to compare.</param>
        /// <returns>
        /// <see langword="true"/> if both descriptions are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Description left, Description right)
            => left.Equals(right);

        /// <summary>
        /// Determines whether two descriptions are different.
        /// </summary>
        /// <param name="left">The first description to compare.</param>
        /// <param name="right">The second description to compare.</param>
        /// <returns>
        /// <see langword="true"/> if both descriptions are different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(Description left, Description right)
            => !left.Equals(right);

        /// <summary>
        /// Implicitly converts a <see cref="Description"/> to <see cref="string"/>.
        /// </summary>
        /// <param name="description">The description to convert.</param>
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