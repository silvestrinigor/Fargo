namespace Fargo.Domain.ValueObjects
{
    /// <summary>
    /// Represents a hashed token stored by the system.
    ///
    /// This value object ensures the stored value is a valid hash
    /// of a security token. The original token should never be
    /// persisted, only its hash.
    /// </summary>
    public readonly struct TokenHash
    {
        /// <summary>
        /// Minimum allowed length for the token hash.
        /// </summary>
        public const int MinLength = 50;

        /// <summary>
        /// Maximum allowed length for the token hash.
        /// </summary>
        public const int MaxLength = 512;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenHash"/> value object.
        /// </summary>
        /// <param name="value">The hashed token value.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the value is null, empty, or contains invalid characters.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the hash length is outside the allowed range.
        /// </exception>
        public TokenHash(string value)
        {
            Validate(value);
            this.value = value;
        }

        /// <summary>
        /// Gets the underlying hash value.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the struct was not properly initialized.
        /// This protects against the default struct state.
        /// </exception>
        public string Value
            => value ?? throw new InvalidOperationException("Token hash value must be set.");

        private readonly string value;

        /// <summary>
        /// Creates a <see cref="TokenHash"/> from the specified string.
        /// </summary>
        public static TokenHash FromString(string value)
            => new(value);

        /// <summary>
        /// Returns the string representation of the token hash.
        /// </summary>
        public override string ToString()
            => Value;

        /// <summary>
        /// Implicitly converts a <see cref="TokenHash"/> to <see cref="string"/>.
        /// </summary>
        public static implicit operator string(TokenHash tokenHash)
            => tokenHash.Value;

        /// <summary>
        /// Explicitly converts a <see cref="string"/> to <see cref="TokenHash"/>.
        /// </summary>
        public static explicit operator TokenHash(string value)
            => new(value);

        /// <summary>
        /// Validates the token hash value.
        /// </summary>
        private static void Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Token hash cannot be null or empty.",
                    nameof(value));
            }

            if (value.Length < MinLength || value.Length > MaxLength)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value.Length,
                    $"Token hash length must be between {MinLength} and {MaxLength} characters.");
            }

            foreach (var c in value)
            {
                if (char.IsWhiteSpace(c))
                {
                    throw new ArgumentException(
                        "Token hash cannot contain whitespace.",
                        nameof(value));
                }
            }
        }
    }
}