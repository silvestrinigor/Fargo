using Fargo.Core.Security;

namespace Fargo.Core.Users;

/// <summary>
/// Represents the authentication information associated with a user.
/// </summary>
public class UserAuthentication
{
    /// <summary>
    /// Gets the unique identifier of the associated user.
    /// </summary>
    public Guid UserGuid { get; private init; }

    /// <summary>
    /// Gets the user that owns this authentication information.
    /// </summary>
    public User User { get; private init; }

    /// <summary>
    /// Gets or sets the hashed password of the user.
    /// </summary>
    public PasswordHash? PasswordHash { get; private set; } = null;

    /// <summary>
    /// Gets or sets the amount of time before a password expires.
    /// </summary>
    public TimeSpan? DefaultPasswordExpirationPeriod { get; set; } = null;

    /// <summary>
    /// Gets or sets the date and time after which the user must change their password.
    /// </summary>
    public DateTimeOffset? RequirePasswordChangeAt { get; set; } = null;

    /// <summary>
    /// Gets a value indicating whether the user is required to change their password.
    /// </summary>
    public bool IsPasswordChangeRequired
        => RequirePasswordChangeAt is not null && DateTimeOffset.UtcNow >= RequirePasswordChangeAt;

    /// <summary>
    /// Gets the current authentication version of the user.
    ///
    /// Changing this value invalidates all previously issued authentication tokens.
    /// </summary>
    public Guid AuthVersion { get; private set; } = Guid.NewGuid();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private UserAuthentication() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    internal UserAuthentication(User user)
    {
        User = user;
        UserGuid = user.Guid;
    }

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
    /// Marks the user's password as requiring an immediate change.
    /// </summary>
    /// <remarks>
    /// After calling this method, <see cref="IsPasswordChangeRequired"/> returns
    /// <see langword="true"/> until the password is changed and its expiration
    /// is reset.
    /// </remarks>
    public void MarkPasswordChangeAsRequired()
    {
        RequirePasswordChangeAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Generates a new authentication version, invalidating all previously issued
    /// authentication tokens.
    /// </summary>
    public void RotateAuthVersion()
    {
        AuthVersion = Guid.NewGuid();
    }

    /// <summary>
    /// Sets the password hash for the entity.
    /// </summary>
    /// <param name="passwordHash">The new password hash.</param>
    public void SetPasswordHash(PasswordHash passwordHash)
    {
        PasswordHash = passwordHash;
    }
}
