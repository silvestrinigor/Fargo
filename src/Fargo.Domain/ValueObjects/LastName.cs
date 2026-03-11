namespace Fargo.Domain.ValueObjects
{
    /// <summary>
    /// Represents a validated last name value object used in the domain.
    ///
    /// This value object guarantees that a last name:
    /// - is not null, empty, or whitespace
    /// - is within the allowed length range
    /// - contains only letters, spaces, or hyphens
    /// - does not start or end with separators
    /// - does not contain consecutive separators
    /// </summary>
    public readonly struct LastName : IEquatable<LastName>
    {
        /// <summary>
        /// Maximum allowed length for a last name.
        /// </summary>
        public const int MaxLength = 100;

        /// <summary>
        /// Minimum allowed length for a last name.
        /// </summary>
        public const int MinLength = 2;

        private readonly string value;

        /// <summary>
        /// Initializes a new instance of the <see cref="LastName"/> struct.
        /// </summary>
        /// <param name="value">The string value representing the last name.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the value is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the value length is outside the allowed range.
        /// </exception>
        public LastName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Last name cannot be null, empty, or whitespace.",
                    nameof(value));
            }

            value = value.Trim();

            if (value.Length < MinLength || value.Length > MaxLength)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    $"Last name length must be between {MinLength} and {MaxLength} characters.");
            }

            ValidateCharacters(value);

            this.value = value;
        }

        /// <summary>
        /// Gets the underlying string value of the last name.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the value object was not properly initialized.
        /// </exception>
        public string Value
            => value ?? throw new InvalidOperationException("LastName not initialized.");

        /// <summary>
        /// Creates a new <see cref="LastName"/> from a string.
        /// </summary>
        public static LastName FromString(string value)
            => new(value);

        /// <summary>
        /// Creates a new validated <see cref="LastName"/>.
        /// </summary>
        public static LastName NewLastName(string value)
            => new(value);

        /// <summary>
        /// Determines whether the current last name is equal to another last name.
        /// </summary>
        public bool Equals(LastName other)
            => string.Equals(value, other.value, StringComparison.Ordinal);

        /// <summary>
        /// Determines whether the current last name is equal to the specified object.
        /// </summary>
        public override bool Equals(object? obj)
            => obj is LastName other && Equals(other);

        /// <summary>
        /// Returns a hash code for the current last name.
        /// </summary>
        public override int GetHashCode()
            => value is null ? 0 : value.GetHashCode(StringComparison.Ordinal);

        /// <summary>
        /// Determines whether two <see cref="LastName"/> instances are equal.
        /// </summary>
        public static bool operator ==(LastName left, LastName right)
            => left.Equals(right);

        /// <summary>
        /// Determines whether two <see cref="LastName"/> instances are different.
        /// </summary>
        public static bool operator !=(LastName left, LastName right)
            => !left.Equals(right);

        /// <summary>
        /// Returns the string representation of the last name.
        /// </summary>
        public override string ToString()
            => Value;

        /// <summary>
        /// Implicitly converts a <see cref="LastName"/> to its string representation.
        /// </summary>
        public static implicit operator string(LastName lastName)
            => lastName.Value;

        /// <summary>
        /// Explicitly converts a string to a <see cref="LastName"/>.
        /// </summary>
        public static explicit operator LastName(string value)
            => new(value);

        private static void ValidateCharacters(string value)
        {
            var previousWasSeparator = false;

            for (var i = 0; i < value.Length; i++)
            {
                var current = value[i];
                var isSeparator = current == ' ' || current == '-';

                if (char.IsLetter(current))
                {
                    previousWasSeparator = false;
                    continue;
                }

                if (isSeparator)
                {
                    if (i == 0 || i == value.Length - 1 || previousWasSeparator)
                    {
                        throw new ArgumentException(
                            "Last name cannot start or end with a separator, or contain consecutive separators.",
                            nameof(value));
                    }

                    previousWasSeparator = true;
                    continue;
                }

                throw new ArgumentException(
                    "Last name can contain only letters, spaces, or hyphens.",
                    nameof(value));
            }
        }
    }
}