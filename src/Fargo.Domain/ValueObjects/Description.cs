namespace Fargo.Domain.ValueObjects
{
    /// <summary>
    /// Represents a validated textual description in the domain.
    ///
    /// This value object allows empty values, but enforces a maximum length.
    /// Because it is implemented as a <see langword="struct"/>, it also safely
    /// handles the default uninitialized state by exposing an empty string.
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

        /// <summary>
        /// Initializes a new empty <see cref="Description"/>.
        /// </summary>
        public Description() : this(string.Empty) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Description"/> value object.
        /// </summary>
        /// <param name="value">The textual description.</param>
        public Description(string value)
        {
            Validate(value);
            this.value = value;
        }

        private readonly string? value;

        /// <summary>
        /// Gets the underlying string value of the description.
        ///
        /// When the struct is in its default uninitialized state,
        /// this property returns an empty string.
        /// </summary>
        public string Value => value ?? string.Empty;

        /// <summary>
        /// Gets an empty description.
        /// </summary>
        public static Description Empty => new(string.Empty);

        /// <summary>
        /// Creates a new <see cref="Description"/> from the specified string.
        /// </summary>
        public static Description FromString(string value)
            => new(value);

        /// <summary>
        /// Determines whether the current description is equal to another.
        /// </summary>
        public bool Equals(Description other)
            => string.Equals(Value, other.Value, StringComparison.Ordinal);

        /// <summary>
        /// Determines whether the current description is equal to the specified object.
        /// </summary>
        public override bool Equals(object? obj)
            => obj is Description other && Equals(other);

        /// <summary>
        /// Returns a hash code for the description.
        /// </summary>
        public override int GetHashCode()
            => Value.GetHashCode(StringComparison.Ordinal);

        /// <summary>
        /// Determines whether two descriptions are equal.
        /// </summary>
        public static bool operator ==(Description left, Description right)
            => left.Equals(right);

        /// <summary>
        /// Determines whether two descriptions are different.
        /// </summary>
        public static bool operator !=(Description left, Description right)
            => !left.Equals(right);

        /// <summary>
        /// Returns the string representation of the description.
        /// </summary>
        public override string ToString()
            => Value;

        /// <summary>
        /// Implicitly converts a <see cref="Description"/> to <see cref="string"/>.
        /// </summary>
        public static implicit operator string(Description description)
            => description.Value;

        /// <summary>
        /// Explicitly converts a <see cref="string"/> to <see cref="Description"/>.
        /// </summary>
        public static explicit operator Description(string value)
            => new(value);

        /// <summary>
        /// Validates the specified description value.
        /// </summary>
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