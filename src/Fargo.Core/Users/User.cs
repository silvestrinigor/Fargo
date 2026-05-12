using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;

namespace Fargo.Core.Users;

#region Value Objects

/// <summary>
/// Represents a validated user identifier (NAMEID) in the domain.
///
/// A NAMEID is used as a unique textual identifier for a user and must
/// follow a restricted set of allowed characters and formatting rules.
/// Comparisons are case-insensitive.
/// </summary>
public readonly struct Nameid : IEquatable<Nameid>
{
    /// <summary>
    /// Maximum allowed length for the identifier.
    /// </summary>
    public const int MaxLength = 100;

    /// <summary>
    /// Minimum allowed length for the identifier.
    /// </summary>
    public const int MinLength = 3;

    private readonly string value;

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
        this.value = value.ToLowerInvariant();
    }

    /// <summary>
    /// Gets the underlying normalized string value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the value object was not properly initialized.
    /// This protects against the default struct state.
    /// </exception>
    public string Value
        => value ?? throw new InvalidOperationException("Nameid not initialized.");

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
    /// Determines whether this instance and another <see cref="Nameid"/> are equal,
    /// ignoring character casing.
    /// </summary>
    public bool Equals(Nameid other)
        => string.Equals(value, other.value, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this instance and a specified object are equal.
    /// </summary>
    public override bool Equals(object? obj)
        => obj is Nameid other && Equals(other);

    /// <summary>
    /// Returns a hash code for this instance, using case-insensitive semantics.
    /// </summary>
    public override int GetHashCode()
        => value is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(value);

    /// <summary>
    /// Determines whether two <see cref="Nameid"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="Nameid"/> to compare.</param>
    /// <param name="right">The second <see cref="Nameid"/> to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both <see cref="Nameid"/> instances are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(Nameid left, Nameid right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Nameid"/> instances are different.
    /// </summary>
    /// <param name="left">The first <see cref="Nameid"/> to compare.</param>
    /// <param name="right">The second <see cref="Nameid"/> to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both <see cref="Nameid"/> instances are different;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(Nameid left, Nameid right)
        => !left.Equals(right);

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
/// <summary>
/// Represents a validated first name value object used in the domain.
///
/// This value object guarantees that a first name:
/// - is not null, empty, or whitespace
/// - is within the allowed length range
/// - contains only letters, spaces, or hyphens
/// - does not start or end with spaces or hyphens
/// - does not contain consecutive spaces or hyphens
/// </summary>
public readonly struct FirstName : IEquatable<FirstName>
{
    /// <summary>
    /// Maximum allowed length for a first name.
    /// </summary>
    public const int MaxLength = 50;

    /// <summary>
    /// Minimum allowed length for a first name.
    /// </summary>
    public const int MinLength = 2;

    private readonly string value;

    /// <summary>
    /// Initializes a new instance of the <see cref="FirstName"/> struct.
    /// </summary>
    /// <param name="value">The string value representing the first name.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is null, empty, or contains only whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value length is outside the allowed range.
    /// </exception>
    public FirstName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "First name cannot be null, empty, or whitespace.",
                nameof(value));
        }

        value = value.Trim();

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"First name length must be between {MinLength} and {MaxLength} characters.");
        }

        ValidateCharacters(value);

        this.value = value;
    }

    /// <summary>
    /// Gets the underlying string value of the first name.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the value object was not properly initialized.
    /// </exception>
    public string Value
        => value ?? throw new InvalidOperationException("First name not initialized.");

    /// <summary>
    /// Creates a new <see cref="FirstName"/> from a string.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>A validated <see cref="FirstName"/> instance.</returns>
    public static FirstName FromString(string value)
        => new(value);

    /// <summary>
    /// Creates a new validated <see cref="FirstName"/>.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>A new <see cref="FirstName"/> instance.</returns>
    public static FirstName NewFirstName(string value)
        => new(value);

    /// <summary>
    /// Determines whether the current first name is equal to another first name.
    /// </summary>
    /// <param name="other">The other first name to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both first names have the same value; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(FirstName other)
        => string.Equals(value, other.value, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the current first name is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with the current first name.</param>
    /// <returns>
    /// <see langword="true"/> if the specified object is a <see cref="FirstName"/> with the same value;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is FirstName other && Equals(other);

    /// <summary>
    /// Returns a hash code for the current first name.
    /// </summary>
    /// <returns>A hash code based on the underlying value.</returns>
    public override int GetHashCode()
        => value is null ? 0 : value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two <see cref="FirstName"/> instances are equal.
    /// </summary>
    public static bool operator ==(FirstName left, FirstName right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="FirstName"/> instances are different.
    /// </summary>
    public static bool operator !=(FirstName left, FirstName right)
        => !left.Equals(right);

    /// <summary>
    /// Returns the string representation of the first name.
    /// </summary>
    public override string ToString()
        => Value;

    /// <summary>
    /// Implicitly converts a <see cref="FirstName"/> to its string representation.
    /// </summary>
    public static implicit operator string(FirstName firstName)
        => firstName.Value;

    /// <summary>
    /// Explicitly converts a string to a <see cref="FirstName"/>.
    /// </summary>
    public static explicit operator FirstName(string value)
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
                        "First name cannot start or end with a separator, or contain consecutive separators.",
                        nameof(value));
                }

                previousWasSeparator = true;
                continue;
            }

            throw new ArgumentException(
                "First name can contain only letters, spaces, or hyphens.",
                nameof(value));
        }
    }
}
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
/// <summary>
/// Represents a validated plaintext password in the domain.
///
/// This value object enforces minimum security rules for passwords
/// before they are hashed and stored in the system.
/// </summary>
public readonly struct Password : IEquatable<Password>
{
    /// <summary>
    /// Maximum allowed length for the password.
    /// </summary>
    public const int MaxLength = 512;

    /// <summary>
    /// Minimum allowed length for the password.
    /// </summary>
    public const int MinLength = 9;

    private readonly string value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Password"/> value object.
    /// </summary>
    /// <param name="value">The plaintext password.</param>
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

    /// <summary>
    /// Creates a new <see cref="Password"/> from a string.
    /// </summary>
    public static Password FromString(string value)
        => new(value);

    /// <summary>
    /// Determines whether the current password is equal to another password.
    /// </summary>
    public bool Equals(Password other)
        => string.Equals(value, other.value, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the current password is equal to the specified object.
    /// </summary>
    public override bool Equals(object? obj)
        => obj is Password other && Equals(other);

    /// <summary>
    /// Returns a hash code for the current password.
    /// </summary>
    public override int GetHashCode()
        => value is null ? 0 : value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two <see cref="Password"/> instances are equal.
    /// </summary>
    public static bool operator ==(Password left, Password right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Password"/> instances are different.
    /// </summary>
    public static bool operator !=(Password left, Password right)
        => !left.Equals(right);

    /// <summary>
    /// Returns the string representation of the password.
    /// </summary>
    /// <remarks>
    /// Use carefully, as this exposes the plaintext password.
    /// </remarks>
    public override string ToString()
        => Value;

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
/// <summary>
/// Represents a hashed password stored in the system.
///
/// This value object guarantees that the stored value is a valid
/// password hash produced by the hashing infrastructure.
/// The plaintext password should never be persisted.
/// </summary>
public readonly struct PasswordHash : IEquatable<PasswordHash>
{
    /// <summary>
    /// Minimum allowed length for the password hash.
    /// </summary>
    public const int MinLength = 50;

    /// <summary>
    /// Maximum allowed length for the password hash.
    /// </summary>
    public const int MaxLength = 512;

    private readonly string value;

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordHash"/> value object.
    /// </summary>
    /// <param name="value">The hashed password value.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is null, empty, or contains invalid characters.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value length is outside the allowed range.
    /// </exception>
    public PasswordHash(string value)
    {
        Validate(value);
        this.value = value;
    }

    /// <summary>
    /// Gets the underlying hash string.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the struct is not properly initialized.
    /// </exception>
    public string Value
        => value ?? throw new InvalidOperationException("Password hash value must be set.");

    /// <summary>
    /// Creates a <see cref="PasswordHash"/> from the specified string.
    /// </summary>
    public static PasswordHash FromString(string value)
        => new(value);

    /// <summary>
    /// Determines whether the current password hash is equal to another password hash.
    /// </summary>
    public bool Equals(PasswordHash other)
        => string.Equals(value, other.value, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the current password hash is equal to the specified object.
    /// </summary>
    public override bool Equals(object? obj)
        => obj is PasswordHash other && Equals(other);

    /// <summary>
    /// Returns a hash code for the current password hash.
    /// </summary>
    public override int GetHashCode()
        => value is null ? 0 : value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two <see cref="PasswordHash"/> instances are equal.
    /// </summary>
    public static bool operator ==(PasswordHash left, PasswordHash right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="PasswordHash"/> instances are different.
    /// </summary>
    public static bool operator !=(PasswordHash left, PasswordHash right)
        => !left.Equals(right);

    /// <summary>
    /// Returns the string representation of the password hash.
    /// </summary>
    public override string ToString()
        => Value;

    /// <summary>
    /// Implicitly converts a <see cref="PasswordHash"/> to <see cref="string"/>.
    /// </summary>
    public static implicit operator string(PasswordHash passwordHash)
        => passwordHash.Value;

    /// <summary>
    /// Explicitly converts a <see cref="string"/> to <see cref="PasswordHash"/>.
    /// </summary>
    public static explicit operator PasswordHash(string value)
        => new(value);

    /// <summary>
    /// Validates the password hash value.
    /// </summary>
    private static void Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Password hash cannot be null or empty.",
                nameof(value));
        }

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value.Length,
                $"Password hash length must be between {MinLength} and {MaxLength} characters.");
        }

        foreach (var c in value)
        {
            if (char.IsWhiteSpace(c))
            {
                throw new ArgumentException(
                    "Password hash cannot contain whitespace.",
                    nameof(value));
            }
        }
    }
}

#endregion Value Objects

#region Entity

/// <summary>
/// Represents a user in the system.
/// </summary>
/// <remarks>
/// A user contains authentication credentials, direct permissions,
/// partition access, and group memberships that may grant additional
/// permissions and access.
///
/// Authorization for a user is determined by the combination of:
/// - Direct permissions and partition access
/// - Permissions and partition access inherited from user groups
/// </remarks>
public class User : ModifiedEntity, IPartitionedEntity, IPartitionUser, IPartitioned, IPermissionUser, IActivable
{
    /// <summary>
    /// Gets or sets the unique NAMEID (username) of the user.
    /// This value identifies the user in the system and must satisfy
    /// the validation rules defined by <see cref="Nameid"/>.
    /// </summary>
    public required Nameid Nameid
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the user's first name.
    ///
    /// This value is optional and, when provided, must satisfy the
    /// validation rules defined by <see cref="FirstName"/>.
    /// </summary>
    /// <remarks>
    /// A <see langword="null"/> value indicates that the first name
    /// has not been specified.
    /// </remarks>
    public FirstName? FirstName
    {
        get;
        set;
    } = null;

    /// <summary>
    /// Gets or sets the user's last name.
    ///
    /// This value is optional and, when provided, must satisfy the
    /// validation rules defined by <see cref="LastName"/>.
    /// </summary>
    /// <remarks>
    /// A <see langword="null"/> value indicates that the last name
    /// has not been specified.
    /// </remarks>
    public LastName? LastName
    {
        get;
        set;
    } = null;

    /// <summary>
    /// Gets or sets the textual description associated with the user.
    /// If not specified, the description defaults to <see cref="Description.Empty"/>.
    /// </summary>
    public Description Description
    {
        get;
        set;
    } = Description.Empty;

    #region Active

    /// <summary>
    /// Gets a value indicating whether the user is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Activates the user.
    /// </summary>
    public void Activate() => IsActive = true;

    /// <summary>
    /// Deactivates the user.
    /// </summary>
    public void Deactivate() => IsActive = false;

    #endregion Active

    #region Password

    /// <summary>
    /// Gets or sets the hashed password of the user.
    ///
    /// The raw password is never stored. Instead, a secure hash
    /// is persisted using the application's password hashing strategy.
    /// </summary>
    public required PasswordHash PasswordHash
    {
        get;
        set;
    }

    public TimeSpan? DefaultPasswordExpirationPeriod { get; set; } = null;

    public DateTimeOffset? RequirePasswordChangeAt { get; set; } = null;

    public Guid AuthVersion { get; private set; } = Guid.NewGuid();

    public bool IsPasswordChangeRequired
        => RequirePasswordChangeAt is not null && DateTimeOffset.UtcNow >= RequirePasswordChangeAt;

    /// <summary>
    /// Resets the password expiration date based on the user's
    /// <see cref="DefaultPasswordExpirationPeriod"/>.
    ///
    /// The new expiration date is calculated by adding the configured
    /// default expiration interval to the current UTC time.
    ///
    /// A value of <see cref="TimeSpan.Zero"/> causes the password to expire
    /// immediately.
    /// </summary>
    /// <remarks>
    /// This method is typically used after the user successfully changes
    /// their own password.
    /// </remarks>
    public void ResetPasswordExpiration()
        => RequirePasswordChangeAt = DateTimeOffset.UtcNow + DefaultPasswordExpirationPeriod;

    /// <summary>
    /// Sets the password expiration requirement to a future date based on the specified number of days.
    /// </summary>
    /// <param name="days">
    /// The number of days from the current UTC time after which the user must change their password.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="days"/> is less than zero.
    /// </exception>
    public void RequirePasswordChangeInDays(int days)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(days);

        RequirePasswordChangeAt = DateTimeOffset.UtcNow.AddDays(days);
    }

    /// <summary>
    /// Marks the user's password as requiring an immediate change.
    /// </summary>
    /// <remarks>
    /// After calling this method, <see cref="IsPasswordChangeRequired"/> will return <c>true</c>
    /// until the password is updated and a new expiration date is set.
    /// </remarks>
    public void MarkPasswordChangeAsRequired()
    {
        RequirePasswordChangeAt = DateTimeOffset.UtcNow;
    }

    public void RotateAuthVersion()
    {
        AuthVersion = Guid.NewGuid();
    }

    #endregion Password

    #region Permission

    /// <summary>
    /// Gets the read-only collection of permissions assigned directly to the user.
    ///
    /// Each permission represents an allowed <see cref="ActionType"/>
    /// that the user can perform without considering group memberships.
    /// </summary>
    public IReadOnlyCollection<UserPermission> Permissions
    {
        get => permissions;
        init => permissions = [.. value];
    }

    /// <inheritdoc />
    IReadOnlyCollection<IPermission> IPermissionUser.Permissions => Permissions;

    private readonly List<UserPermission> permissions = [];

    /// <summary>
    /// Adds a permission to the user if it does not already exist.
    /// </summary>
    /// <param name="action">The action type to allow.</param>
    public void AddPermission(ActionType action)
    {
        if (permissions.Any(x => x.Action == action))
        {
            return;
        }

        var userPermission = new UserPermission
        {
            Action = action,
            User = this
        };

        permissions.Add(userPermission);
    }

    /// <summary>
    /// Removes a permission from the user if it exists.
    /// </summary>
    /// <param name="action">The action type to remove.</param>
    public void RemovePermission(ActionType action)
    {
        var userPermission = permissions.SingleOrDefault(x => x.Action == action);

        if (userPermission == null)
        {
            return;
        }

        permissions.Remove(userPermission);
    }

    #endregion Permission

    /// <summary>
    /// Gets the collection of groups the user belongs to.
    /// </summary>
    /// <remarks>
    /// User groups provide additional permissions and partition access
    /// that are inherited by the user.
    ///
    /// Effective authorization for a user is typically the combination of:
    /// - Direct permissions and partition access
    /// - Permissions and partition access inherited from groups
    /// </remarks>
    public UserGroupCollection UserGroups { get; init; } = [];

    #region Partition

    /// <summary>
    /// Gets the read-only collection of partitions the user has access to.
    /// </summary>
    /// <remarks>
    /// Partitions define logical boundaries in the system.
    /// A user can access entities that have no partition (public), or that
    /// belong to at least one partition to which the user has been granted access.
    /// </remarks>
    public IReadOnlyCollection<UserPartitionAccess> PartitionAccesses
    {
        get => partitionAccesses;
        init => partitionAccesses = [.. value];
    }

    /// <inheritdoc />
    IReadOnlyCollection<IPartitionAccess> IPartitionUser.PartitionAccesses => PartitionAccesses;

    private readonly List<UserPartitionAccess> partitionAccesses = [];

    /// <summary>
    /// Grants access to the specified partition for the user.
    /// </summary>
    /// <param name="partition">The partition to grant access to.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="partition"/> is <see langword="null"/>.
    /// </exception>
    public void AddPartitionAccess(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (partitionAccesses.Any(x => x.PartitionGuid == partition.Guid))
        {
            return;
        }

        var partitionAccess = new UserPartitionAccess
        {
            User = this,
            Partition = partition
        };

        partitionAccesses.Add(partitionAccess);
    }

    /// <summary>
    /// Removes access to the specified partition from the user.
    /// </summary>
    /// <param name="partitionGuid">The partition identifier.</param>
    public void RemovePartitionAccess(Guid partitionGuid)
    {
        var userPartition =
            partitionAccesses.SingleOrDefault(x => x.PartitionGuid == partitionGuid);

        if (userPartition == null)
        {
            return;
        }

        partitionAccesses.Remove(userPartition);
    }

    /// <summary>
    /// Gets the partitions associated with the user entity.
    /// </summary>
    /// <remarks>
    /// These partitions define the partition scope of the user entity itself,
    /// not the partitions the user has access to.
    ///
    /// This is used for partition-based access control on the user entity,
    /// meaning a user can only access this user record if they have access
    /// to at least one of these partitions.
    ///
    /// To determine which partitions the user can access, see
    /// <see cref="PartitionAccesses"/> and <see cref="UserGroups"/>.
    /// </remarks>
    public PartitionCollection Partitions { get; init; } = [];

    /// <inheritdoc />
    IReadOnlyCollection<IPartitionEntity> IPartitionedEntity.Partitions => Partitions;

    public IReadOnlyCollection<Guid> PartitionGuids => [.. Partitions.Select(p => p.Guid)];

    #endregion Partition
}

#endregion Entity

#region Permissions

/// <summary>
/// Represents an object that grants a specific permission action.
/// </summary>
/// <remarks>
/// This abstraction allows different permission sources, such as direct user
/// permissions or group permissions, to be evaluated uniformly.
/// </remarks>
public interface IPermission
{
    /// <summary>
    /// Gets the action granted by the permission.
    /// </summary>
    ActionType Action { get; }
}
/// <summary>
/// Represents an object that exposes a read-only collection of permissions.
/// </summary>
/// <remarks>
/// Implementations typically include domain entities such as users or user groups
/// that participate in authorization checks.
/// </remarks>
public interface IPermissionUser
{
    /// <summary>
    /// Gets the read-only collection of permissions associated with the object.
    /// </summary>
    IReadOnlyCollection<IPermission> Permissions { get; }
}
/// <summary>
/// Represents a permission granted to a user or role within the system.
/// </summary>
/// <remarks>
/// A permission defines an action that can be performed in the system.
/// This value object is typically used to represent authorization rules
/// associated with users, groups, or other security-related entities.
/// </remarks>
/// <param name="Guid">
/// The unique identifier of the permission.
/// </param>
/// <param name="Action">
/// The action that the permission allows to be performed.
/// </param>
public sealed record Permission(
    Guid Guid,
    ActionType Action
);
/// <summary>
/// Represents a permission assigned to a user.
/// </summary>
/// <remarks>
/// Each instance defines that a specific <see cref="User"/> is allowed
/// to perform a particular <see cref="ActionType"/>.
///
/// This entity is part of the <see cref="User"/> aggregate and represents
/// a single permission entry associated with the user.
///
/// The entity also implements <see cref="IModifiedEntityMember"/>, meaning
/// that any changes to this permission will propagate auditing updates
/// to the parent <see cref="User"/> entity.
/// </remarks>
public class UserPermission : Entity, IModifiedEntityMember, IPermission
{
    /// <summary>
    /// Gets the unique identifier of the user that owns this permission.
    /// </summary>
    /// <remarks>
    /// This value mirrors the identifier of the associated <see cref="User"/>.
    /// It is automatically synchronized when the <see cref="User"/> property
    /// is assigned.
    /// </remarks>
    public Guid UserGuid { get; private set; }

    /// <summary>
    /// Gets the user associated with this permission.
    /// </summary>
    /// <remarks>
    /// When the user is assigned, the <see cref="UserGuid"/> property
    /// is automatically synchronized with the user's identifier.
    ///
    /// This navigation property represents the parent entity in the
    /// aggregate relationship.
    /// </remarks>
    public required User User
    {
        get;
        init
        {
            UserGuid = value.Guid;
            field = value;
        }
    }

    /// <summary>
    /// Gets the action that the user is allowed to perform.
    /// </summary>
    /// <remarks>
    /// Each permission grants the associated user the ability to perform
    /// the specified <see cref="ActionType"/>.
    /// </remarks>
    public required ActionType Action { get; init; }

    /// <summary>
    /// Gets the parent audited entity whose audit metadata must be updated
    /// when this permission changes.
    /// </summary>
    /// <remarks>
    /// Since permissions are part of the <see cref="User"/> aggregate,
    /// modifications to this entity should update the audit metadata of
    /// the parent <see cref="User"/>.
    /// </remarks>
    public IModifiedEntity ParentEditedEntity => User;
}

#endregion Permissions

#region Partition Access

/// <summary>
/// Represents the access relationship between a <see cref="User"/> and a <see cref="Partition"/>.
/// </summary>
/// <remarks>
/// A <see cref="UserPartitionAccess"/> defines whether a user is allowed to access a specific
/// partition and optionally whether the user can modify entities within that partition.
///
/// Partitions are used to logically isolate data in the system. Users can only access
/// entities that belong to partitions for which they have an associated
/// <see cref="UserPartitionAccess"/>.
///
/// This entity is a member of the <see cref="User"/> aggregate and implements
/// <see cref="IModifiedEntityMember"/>, meaning that any modification to this
/// entity should update the audit metadata of the parent <see cref="User"/>.
/// </remarks>
public class UserPartitionAccess : Entity, IModifiedEntityMember, IPartitionAccess
{
    /// <summary>
    /// Gets the unique identifier of the user associated with this access entry.
    /// </summary>
    public Guid UserGuid { get; private set; }

    /// <summary>
    /// Gets or sets the user that owns this partition access.
    /// </summary>
    /// <remarks>
    /// When the user reference is assigned, <see cref="UserGuid"/> is automatically
    /// synchronized with the user's identifier.
    /// </remarks>
    public required User User
    {
        get;
        set
        {
            UserGuid = value.Guid;
            field = value;
        }
    }

    /// <summary>
    /// Gets the unique identifier of the partition associated with this access entry.
    /// </summary>
    public Guid PartitionGuid { get; private set; }

    /// <summary>
    /// Gets or sets the partition to which the user has access.
    /// </summary>
    /// <remarks>
    /// When the partition reference is assigned, <see cref="PartitionGuid"/>
    /// is automatically synchronized with the partition's identifier.
    /// </remarks>
    public required Partition Partition
    {
        get;
        set
        {
            PartitionGuid = value.Guid;
            field = value;
        }
    }

    /// <summary>
    /// Gets the parent audited entity whose audit metadata must be updated
    /// when this entity changes.
    /// </summary>
    /// <remarks>
    /// Since <see cref="UserPartitionAccess"/> belongs to the <see cref="User"/> aggregate,
    /// the parent audited entity is the associated user.
    /// </remarks>
    public IModifiedEntity ParentEditedEntity => User;
}

#endregion Partition Access

#region Actor

/// <summary>
/// Represents an actor corresponding to a real authenticated <see cref="User"/>.
/// </summary>
/// <remarks>
/// This actor is used when an operation is initiated by a real user.
/// <para>
/// The actor's permissions are computed as the union of:
/// <list type="bullet">
/// <item>
/// <description>Permissions directly assigned to the user</description>
/// </item>
/// <item>
/// <description>Permissions inherited from all groups the user belongs to</description>
/// </item>
/// </list>
/// </para>
/// <para>
/// The actor's partition access is derived from:
/// <list type="bullet">
/// <item>
/// <description>Partitions directly assigned to the user</description>
/// </item>
/// <item>
/// <description>Partitions assigned through the user's groups</description>
/// </item>
/// </list>
/// These accesses are typically expanded to include descendant partitions
/// by the request authorization context before the snapshot is used.
/// </para>
/// </remarks>
public sealed class UserActor : Actor
{
    /// <summary>
    /// Initializes a new instance of <see cref="UserActor"/>.
    /// </summary>
    /// <param name="user">
    /// The user associated with the actor.
    /// </param>
    /// <param name="partitionAccesses">
    /// The collection of partition identifiers the actor has access to,
    /// including inherited and expanded (descendant) partitions.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="user"/> is null.
    /// </exception>
    internal UserActor(User user, IReadOnlyCollection<Guid> partitionAccesses)
    {
        ArgumentNullException.ThrowIfNull(user);

        User = user;
        PartitionAccessesGuids = partitionAccesses;
    }

    /// <summary>
    /// Gets the unique identifier of the actor.
    /// </summary>
    public override Guid Guid => User.Guid;

    /// <summary>
    /// Gets the associated <see cref="User"/>.
    /// </summary>
    public User User { get; }

    /// <summary>
    /// Gets a value indicating whether the actor has administrative privileges.
    /// </summary>
    /// <remarks>
    /// A user is considered an administrator when its identifier matches
    /// the default administrator defined by <see cref="UserService"/>.
    /// </remarks>
    public override bool IsAdmin => Guid == UserService.DefaultAdministratorUserGuid;

    public override bool IsActive => User.IsActive;

    /// <summary>
    /// Gets the set of permission actions available to the actor.
    /// </summary>
    /// <remarks>
    /// This is computed as the union of permissions directly assigned to the user
    /// and those inherited from all user groups.
    /// </remarks>
    public override IReadOnlyCollection<ActionType> PermissionActions
    {
        get
        {
            var permissions = new HashSet<ActionType>(
                User.Permissions.Select(p => p.Action));

            foreach (var group in User.UserGroups.Where(group => group.IsActive))
            {
                permissions.UnionWith(group.Permissions.Select(p => p.Action));
            }

            return permissions;
        }
    }

    /// <summary>
    /// Gets the collection of partition identifiers the actor has access to.
    /// </summary>
    /// <remarks>
    /// This includes partitions assigned directly to the user and those inherited
    /// from user groups, already expanded to include descendant partitions.
    /// </remarks>
    public override IReadOnlyCollection<Guid> PartitionAccessesGuids { get; }
}

#endregion Actor

#region Repositories

/// <summary>
/// Defines the repository contract for managing <see cref="User"/> entities.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by its unique identifier.
    /// </summary>
    Task<User?> GetByGuid(
        Guid entityGuid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a user by their unique <see cref="Nameid"/>.
    /// </summary>
    Task<User?> GetByNameid(
        Nameid nameid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Determines whether a user with the specified identifier exists.
    /// </summary>
    Task<bool> ExistsByGuid(
        Guid guid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Determines whether a user with the specified <see cref="Nameid"/> already exists.
    /// </summary>
    Task<bool> ExistsByNameid(
        Nameid nameid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new user to the persistence context.
    /// </summary>
    void Add(User user);

    /// <summary>
    /// Removes a user from the persistence context.
    /// </summary>
    void Remove(User user);

    /// <summary>
    /// Determines whether any users exist in the system.
    /// </summary>
    Task<bool> Any(CancellationToken cancellationToken = default);
}

#endregion Repositories

#region Services

/// <summary>
/// Defines the contract for password hashing operations.
///
/// Implementations are responsible for securely hashing plaintext
/// passwords and verifying provided passwords against stored hashes.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Generates a secure hash for the specified password.
    /// </summary>
    /// <param name="password">The plaintext password string.</param>
    /// <returns>A <see cref="PasswordHash"/> representing the hashed password.</returns>
    PasswordHash Hash(string password);

    /// <summary>
    /// Verifies whether the provided password matches the stored hash.
    /// </summary>
    /// <param name="hashedPassword">The stored password hash.</param>
    /// <param name="providedPassword">The plaintext password string provided for verification.</param>
    /// <returns>
    /// <see langword="true"/> if the password matches the hash; otherwise, <see langword="false"/>.
    /// </returns>
    bool Verify(PasswordHash hashedPassword, string providedPassword);
}
/// <summary>
/// Provides domain validation and business rules related to <see cref="User"/> entities.
/// </summary>
/// <remarks>
/// This service encapsulates domain rules involving users, such as uniqueness
/// validation, and self-protection rules.
/// </remarks>
public class UserService(
    IUserRepository userRepository)
{
    /// <summary>
    /// The predefined unique identifier string representing
    /// the default administrator user.
    /// </summary>
    private const string DefaultAdministratorUserGuidString =
        "00000000-0000-0000-0000-000000000004";

    /// <summary>
    /// Gets the predefined unique identifier representing
    /// the default administrator user.
    /// </summary>
    /// <remarks>
    /// This GUID is reserved for the built-in administrator account
    /// created during system initialization.
    /// </remarks>
    public static Guid DefaultAdministratorUserGuid =>
        new(DefaultAdministratorUserGuidString);

    /// <summary>
    /// Validates the rules required to create a new <see cref="User"/>.
    /// </summary>
    /// <param name="user">
    /// The user being created.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="user"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="UserNameidAlreadyExistsDomainException">
    /// Thrown when another user with the same <see cref="User.Nameid"/> already exists.
    /// </exception>
    /// <remarks>
    /// This validation ensures that the <see cref="User.Nameid"/> is unique
    /// within the system.
    /// </remarks>
    public async Task ValidateUserCreate(
        User user,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var alreadyExistsWithNameid =
            await userRepository.ExistsByNameid(user.Nameid, cancellationToken);

        if (alreadyExistsWithNameid)
        {
            throw new UserNameidAlreadyExistsDomainException(user.Nameid);
        }
    }

    public async Task ValidateUserNameidChange(
        User user,
        Nameid nameid,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (user.Nameid == nameid)
        {
            return;
        }

        var alreadyExistsWithNameid =
            await userRepository.ExistsByNameid(nameid, cancellationToken);

        if (alreadyExistsWithNameid)
        {
            throw new UserNameidAlreadyExistsDomainException(nameid);
        }
    }

    /// <summary>
    /// Validates the rules required to delete a <see cref="User"/>.
    /// </summary>
    /// <param name="user">
    /// The user being deleted.
    /// </param>
    /// <param name="actorGuid">
    /// The identifier of the user performing the delete operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="user"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="UserCannotDeleteSelfFargoDomainException">
    /// Thrown when the acting user attempts to delete their own account.
    /// </exception>
    /// <remarks>
    /// This validation ensures that a user cannot delete their own account.
    /// </remarks>
    public static void ValidateUserDelete(User user, Guid actorGuid)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (user.Guid == actorGuid)
        {
            throw new UserCannotDeleteSelfFargoDomainException(actorGuid);
        }

        if (user.Guid == DefaultAdministratorUserGuid)
        {
            throw new DeleteMainAdminUserFargoDomainException();
        }
    }

    /// <summary>
    /// Validates the rules required to change a user's permissions.
    /// </summary>
    /// <param name="user">
    /// The user whose permissions are being modified.
    /// </param>
    /// <param name="actorGuid">
    /// The identifier of the user performing the permission change operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="user"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="UserCannotChangeOwnPermissionsFargoDomainException">
    /// Thrown when the acting user attempts to modify their own permissions.
    /// </exception>
    /// <remarks>
    /// This validation ensures that a user cannot modify their own permissions.
    /// </remarks>
    public static void ValidateUserPermissionChange(User user, Guid actorGuid)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (user.Guid == actorGuid)
        {
            throw new UserCannotChangeOwnPermissionsFargoDomainException(actorGuid);
        }

        if (user.Guid == DefaultAdministratorUserGuid)
        {
            throw new ChangeMainAdminUserPermissionsFargoDomainException();
        }
    }
}

#endregion Services

#region Exceptions

/// <summary>
/// Exception thrown when an attempt is made to modify the permissions
/// of the main administrator user.
/// </summary>
/// <remarks>
/// The main administrator user has fixed permissions that cannot be altered.
/// </remarks>
public sealed class ChangeMainAdminUserPermissionsFargoDomainException()
    : FargoDomainException("The permissions of the main administrator user cannot be modified.")
{
}
/// <summary>
/// Exception thrown when an attempt is made to delete the main administrator user.
/// </summary>
/// <remarks>
/// The main administrator user is a critical system entity and cannot be deleted.
/// </remarks>
public sealed class DeleteMainAdminUserFargoDomainException()
    : FargoDomainException("The main administrator user cannot be deleted.")
{
}
/// <summary>
/// Exception thrown when a user attempts to change their own permissions.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserCannotChangeOwnPermissionsFargoDomainException"/> class.
/// </remarks>
/// <param name="userGuid">
/// The identifier of the user who attempted to change their own permissions.
/// </param>
public sealed class UserCannotChangeOwnPermissionsFargoDomainException(Guid userGuid)
    : FargoDomainException($"User '{userGuid}' cannot change their own permissions.")
{
    /// <summary>
    /// Gets the identifier of the user who attempted to change their own permissions.
    /// </summary>
    public Guid UserGuid { get; } = userGuid;
}
/// <summary>
/// Exception thrown when a user attempts to delete a user group
/// that they belong to.
/// </summary>
/// <remarks>
/// This rule prevents users from accidentally removing a group
/// that grants their own permissions, which could result in
/// privilege loss or inconsistent authorization state.
/// </remarks>
/// <param name="userGroupGuid">
/// The unique identifier of the user group that the user attempted to delete.
/// </param>
public sealed class UserCannotDeleteParentUserGroupFargoDomainException(Guid userGroupGuid)
    : FargoDomainException($"The user cannot delete the user group '{userGroupGuid}' because they belong to it.")
{
    /// <summary>
    /// Gets the unique identifier of the user group that cannot be deleted.
    /// </summary>
    public Guid UserGroupGuid { get; } = userGroupGuid;
}
/// <summary>
/// Exception thrown when a user attempts to delete their own account.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserCannotDeleteSelfFargoDomainException"/> class.
/// </remarks>
/// <param name="userGuid">
/// The identifier of the user that attempted to delete themselves.
/// </param>
public sealed class UserCannotDeleteSelfFargoDomainException(Guid userGuid)
    : FargoDomainException($"User '{userGuid}' cannot delete their own account.")
{
    /// <summary>
    /// Gets the identifier of the user that attempted to delete themselves.
    /// </summary>
    public Guid UserGuid { get; } = userGuid;
}
/// <summary>
/// Exception thrown when an operation is attempted with an inactive user.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserInactiveFargoDomainException"/> class.
/// </remarks>
/// <param name="userGuid">
/// The identifier of the inactive user.
/// </param>
public sealed class UserInactiveFargoDomainException(Guid userGuid)
    : FargoDomainException($"The user '{userGuid}' is inactive and cannot perform this operation.")
{
    /// <summary>
    /// Gets the GUID of the user that is inactive.
    /// </summary>
    public Guid UserGuid { get; } = userGuid;
}
/// <summary>
/// Represents an error that occurs when attempting to create
/// a <c>User</c> with a <see cref="Nameid"/> that already exists.
///
/// In the domain, a <see cref="Nameid"/> must be unique
/// across all users.
/// </summary>
public sealed class UserNameidAlreadyExistsDomainException(Nameid nameid)
    : FargoDomainException($"A user with Nameid '{nameid}' already exists.")
{
    /// <summary>
    /// Gets the <see cref="Nameid"/> that caused the conflict.
    /// </summary>
    public Nameid Nameid { get; } = nameid;
}
/// <summary>
/// Exception thrown when a user attempts to perform an action
/// for which they do not have permission.
/// </summary>
public sealed class UserNotAuthorizedFargoDomainException(
    Guid userGuid,
    ActionType actionType
    ) : FargoDomainException(
        $"User '{userGuid}' is not authorized to perform action '{actionType}'.")
{
    /// <summary>
    /// Gets the identifier of the user that attempted the action.
    /// </summary>
    public Guid UserGuid { get; } = userGuid;

    /// <summary>
    /// Gets the action the user attempted to perform.
    /// </summary>
    public ActionType ActionType { get; } = actionType;
}

#endregion Exceptions
