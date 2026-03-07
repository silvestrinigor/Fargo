namespace Fargo.Domain.ValueObjects
{
    /// <summary>
    /// Represents a security token used by the system.
    ///
    /// Tokens are typically generated for authentication or authorization
    /// purposes (for example, access tokens or refresh tokens).
    /// This value object ensures the token is not null, empty, or malformed.
    /// </summary>
    public readonly struct Token
    {
        /// <summary>
        /// Minimum allowed length for the token.
        /// </summary>
        public const int MinLength = 50;

        /// <summary>
        /// Maximum allowed length for the token.
        /// </summary>
        public const int MaxLength = 512;

        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> value object.
        /// </summary>
        /// <param name="value">The token string.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the value is null, empty, or contains invalid characters.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the token length is outside the allowed range.
        /// </exception>
        public Token(string value)
        {
            Validate(value);
            this.value = value;
        }

        /// <summary>
        /// Gets the underlying token value.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the struct is not properly initialized.
        /// This protects against the default struct state.
        /// </exception>
        public string Value
            => value ?? throw new InvalidOperationException("Token value must be set.");

        private readonly string value;

        /// <summary>
        /// Creates a <see cref="Token"/> from the specified string.
        /// </summary>
        public static Token FromString(string value)
            => new(value);

        /// <summary>
        /// Returns the token string.
        /// </summary>
        public override string ToString()
            => Value;

        /// <summary>
        /// Implicitly converts a <see cref="Token"/> to <see cref="string"/>.
        /// </summary>
        public static implicit operator string(Token token)
            => token.Value;

        /// <summary>
        /// Explicitly converts a <see cref="string"/> to <see cref="Token"/>.
        /// </summary>
        public static explicit operator Token(string value)
            => new(value);

        /// <summary>
        /// Validates the token value.
        /// </summary>
        private static void Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Token cannot be null or empty.",
                    nameof(value));
            }

            if (value.Length < MinLength || value.Length > MaxLength)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value.Length,
                    $"Token length must be between {MinLength} and {MaxLength} characters.");
            }

            foreach (var c in value)
            {
                if (char.IsWhiteSpace(c))
                {
                    throw new ArgumentException(
                        "Token cannot contain whitespace.",
                        nameof(value));
                }
            }
        }
    }
}