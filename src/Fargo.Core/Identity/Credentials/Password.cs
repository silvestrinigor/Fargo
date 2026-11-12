namespace Fargo.Core.Identity.Credentials;

/// <summary>
/// A plaintext password: 9 to 512 characters, no spaces, with at least one letter, one digit and
/// one special (non-letter, non-digit) character.
/// </summary>
public readonly struct Password : IEquatable<Password>
{
    public const int MaxLength = 512;

    public const int MinLength = 9;

    private readonly string value;

    public Password(string value)
    {
        Validate(value);
        this.value = value;
    }

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> for a default (uninitialized) instance.
    /// </summary>
    public string Value
        => value ?? throw new InvalidOperationException("Password value must be set.");

    public static Password FromString(string value)
        => new(value);

    public bool Equals(Password other)
        => string.Equals(value, other.value, StringComparison.Ordinal);

    public override bool Equals(object? obj)
        => obj is Password other && Equals(other);

    public override int GetHashCode()
        => value is null ? 0 : value.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(Password left, Password right)
        => left.Equals(right);

    public static bool operator !=(Password left, Password right)
        => !left.Equals(right);

    /// <summary>
    /// Returns the plaintext password; take care not to log it.
    /// </summary>
    public override string ToString()
        => Value;

    public static implicit operator string(Password password)
        => password.Value;

    public static explicit operator Password(string value)
        => new(value);

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
            {
                hasLetter = true;
            }
            else if (char.IsDigit(c))
            {
                hasDigit = true;
            }
            else
            {
                hasSpecial = true;
            }
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
