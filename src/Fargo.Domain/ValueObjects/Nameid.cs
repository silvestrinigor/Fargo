namespace Fargo.Domain.ValueObjects
{
    /// <summary>
    /// Represents a validated user identifier (NAMEID) in the domain.
    ///
    /// A NAMEID is used as a unique textual identifier for a user and must
    /// follow a restricted set of allowed characters and formatting rules.
    /// </summary>
    public readonly struct Nameid
    {
        /// <summary>
        /// Maximum allowed length for the identifier.
        /// </summary>
        public const int MaxLength = 100;

        /// <summary>
        /// Minimum allowed length for the identifier.
        /// </summary>
        public const int MinLength = 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="Nameid"/> value object.
        /// </summary>
        /// <param name="value">The identifier string.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the value is invalid.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the value length is outside the allowed range.
        /// </exception>
        public Nameid(string value)
        {
            Validate(value);
            this.value = value;
        }

        /// <summary>
        /// Gets the underlying string value.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the value object was not properly initialized.
        /// This protects against the default struct state.
        /// </exception>
        public string Value
            => value ?? throw new InvalidOperationException("Nameid not initialized.");

        private readonly string value;

        /// <summary>
        /// Creates a new <see cref="Nameid"/> from a string.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <returns>A validated <see cref="Nameid"/> instance.</returns>
        public static Nameid FromString(string value)
            => new(value);

        /// <summary>
        /// Creates a new validated <see cref="Nameid"/>.
        /// </summary>
        /// <param name="value">The identifier value.</param>
        /// <returns>A validated <see cref="Nameid"/> instance.</returns>
        public static Nameid NewNameid(string value)
            => new(value);

        /// <summary>
        /// Returns the string representation of the identifier.
        /// </summary>
        public override string ToString()
            => Value;

        /// <summary>
        /// Implicitly converts a <see cref="Nameid"/> to <see cref="string"/>.
        /// </summary>
        public static implicit operator string(Nameid nameid)
            => nameid.Value;

        /// <summary>
        /// Explicitly converts a <see cref="string"/> to <see cref="Nameid"/>.
        /// </summary>
        public static explicit operator Nameid(string value)
            => new(value);

        /// <summary>
        /// Validates the identifier value.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        private static void Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Nameid cannot be null, empty, or whitespace.",
                    nameof(value));
            }

            if (value.Length < MinLength || value.Length > MaxLength)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    $"Nameid length must be between {MinLength} and {MaxLength} characters.");
            }

            if (value != value.Trim())
            {
                throw new ArgumentException(
                    "Nameid cannot start or end with whitespace.",
                    nameof(value));
            }

            if (value.Contains(' '))
            {
                throw new ArgumentException(
                    "Nameid cannot contain spaces.",
                    nameof(value));
            }

            if (!char.IsLetterOrDigit(value[0]))
            {
                throw new ArgumentException(
                    "Nameid must start with a letter or digit.",
                    nameof(value));
            }

            if (!char.IsLetterOrDigit(value[^1]))
            {
                throw new ArgumentException(
                    "Nameid must end with a letter or digit.",
                    nameof(value));
            }

            for (var i = 0; i < value.Length; i++)
            {
                var current = value[i];
                var isAllowed =
                    char.IsLetterOrDigit(current) ||
                    current == '.' ||
                    current == '_' ||
                    current == '-';

                if (!isAllowed)
                {
                    throw new ArgumentException(
                        "Nameid can only contain letters, digits, '.', '_' and '-'.",
                        nameof(value));
                }

                if (i > 0)
                {
                    var previous = value[i - 1];
                    var currentIsSeparator = current == '.' || current == '_' || current == '-';
                    var previousIsSeparator = previous == '.' || previous == '_' || previous == '-';

                    if (currentIsSeparator && previousIsSeparator)
                    {
                        throw new ArgumentException(
                            "Nameid cannot contain consecutive special characters.",
                            nameof(value));
                    }
                }
            }
        }
    }
}