namespace Fargo.Core;

/// <summary>
/// Represents a validated name value object used across the domain.
///
/// This value object guarantees that a name is always within the
/// allowed length range and is not null, empty, or composed only of whitespace.
/// </summary>
public readonly struct Name : IEquatable<Name>
{
    /// <summary>
    /// Maximum allowed length for a name.
    /// </summary>
    public const int MaxLength = 100;

    /// <summary>
    /// Minimum allowed length for a name.
    /// </summary>
    public const int MinLength = 3;

    private readonly string value;

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
        {
            throw new ArgumentException("Name cannot be null, empty, or whitespace.", nameof(value));
        }

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Name length must be between {MinLength} and {MaxLength} characters."
            );
        }

        this.value = value;
    }

    /// <summary>
    /// Gets the underlying string value of the name.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the value object was not properly initialized.
    /// </exception>
    public string Value
        => value ?? throw new InvalidOperationException("Name not initialized.");

    /// <summary>
    /// Creates a new <see cref="Name"/> from a string.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>A validated <see cref="Name"/> instance.</returns>
    public static Name FromString(string value)
        => new(value);

    /// <summary>
    /// Creates a new validated <see cref="Name"/>.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>A new <see cref="Name"/> instance.</returns>
    public static Name NewName(string value)
        => new(value);

    /// <summary>
    /// Determines whether the current name is equal to another name.
    /// </summary>
    /// <param name="other">The other name to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both names have the same value; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(Name other)
        => string.Equals(value, other.value, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the current name is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with the current name.</param>
    /// <returns>
    /// <see langword="true"/> if the specified object is a <see cref="Name"/> with the same value;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is Name other && Equals(other);

    /// <summary>
    /// Returns a hash code for the current name.
    /// </summary>
    /// <returns>A hash code based on the underlying value.</returns>
    public override int GetHashCode()
        => value is null ? 0 : value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two <see cref="Name"/> instances are equal.
    /// </summary>
    /// <param name="left">The first name to compare.</param>
    /// <param name="right">The second name to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both names are equal; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(Name left, Name right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Name"/> instances are different.
    /// </summary>
    /// <param name="left">The first name to compare.</param>
    /// <param name="right">The second name to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both names are different; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(Name left, Name right)
        => !left.Equals(right);

    /// <summary>
    /// Returns the string representation of the name.
    /// </summary>
    /// <returns>The underlying string value.</returns>
    public override string ToString()
        => Value;

    /// <summary>
    /// Implicitly converts a <see cref="Name"/> to its string representation.
    /// </summary>
    public static implicit operator string(Name name)
        => name.Value;

    /// <summary>
    /// Explicitly converts a string to a <see cref="Name"/>.
    /// </summary>
    public static explicit operator Name(string value)
        => new(value);
}
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
