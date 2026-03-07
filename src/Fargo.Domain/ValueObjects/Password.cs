namespace Fargo.Domain.ValueObjects
{
    /// <summary>
    /// Represents a validated plaintext password in the domain.
    ///
    /// This value object enforces minimum security rules for passwords
    /// before they are hashed and stored in the system.
    /// </summary>
    public readonly struct Password
    {
        /// <summary>
        /// Maximum allowed length for the password.
        /// </summary>
        public const int MaxLength = 512;

        /// <summary>
        /// Minimum allowed length for the password.
        /// </summary>
        public const int MinLength = 9;

        /// <summary>
        /// Initializes a new instance of the <see cref="Password"/> value object.
        /// </summary>
        /// <param name="value">The plaintext password.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the password is null, empty, or contains invalid characters.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the password length is outside the allowed range.
        /// </exception>
        public Password(string value)
        {
            Validate(value);
            this.value = value;
        }

        /// <summary>
        /// Gets the underlying password string.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the password was not properly initialized.
        /// This protects against the default struct state.
        /// </exception>
        public string Value
            => value ?? throw new InvalidOperationException("Password value must be set.");

        private readonly string value;

        /// <summary>
        /// Creates a new <see cref="Password"/> from a string.
        /// </summary>
        /// <param name="value">The plaintext password.</param>
        /// <returns>A validated <see cref="Password"/> instance.</returns>
        public static Password FromString(string value)
            => new(value);

        /// <summary>
        /// Implicitly converts a <see cref="Password"/> to <see cref="string"/>.
        /// </summary>
        public static implicit operator string(Password password)
            => password.Value;

        /// <summary>
        /// Explicitly converts a <see cref="string"/> to <see cref="Password"/>.
        /// </summary>
        public static explicit operator Password(string value)
            => new(value);

        /// <summary>
        /// Validates the password value.
        /// </summary>
        /// <param name="value">The password to validate.</param>
        private static void Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Password cannot be null, empty, or whitespace.",
                    nameof(value));
            }

            if (value.Length < MinLength || value.Length > MaxLength)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    $"Password length must be between {MinLength} and {MaxLength} characters.");
            }

            if (value.Contains(' '))
            {
                throw new ArgumentException(
                    "Password cannot contain spaces.",
                    nameof(value));
            }

            bool hasLetter = false;
            bool hasDigit = false;
            bool hasSpecial = false;

            foreach (var c in value)
            {
                if (char.IsLetter(c))
                    hasLetter = true;
                else if (char.IsDigit(c))
                    hasDigit = true;
                else
                    hasSpecial = true;
            }

            if (!hasLetter)
            {
                throw new ArgumentException(
                    "Password must contain at least one letter.",
                    nameof(value));
            }

            if (!hasDigit)
            {
                throw new ArgumentException(
                    "Password must contain at least one digit.",
                    nameof(value));
            }

            if (!hasSpecial)
            {
                throw new ArgumentException(
                    "Password must contain at least one special character.",
                    nameof(value));
            }
        }
    }
}