namespace Fargo.Core.Users;

public class UserAuthentication
{
    public Guid UserGuid { get; private init; }

    public User User { get; private init; }

    /// <summary>
    /// Gets or sets the hashed password of the user.
    /// </summary>
    public PasswordHash? PasswordHash { get; set; } = null;

    /// <summary>
    /// Gets or sets the default password expiration perid.
    /// </summary>
    public TimeSpan? DefaultPasswordExpirationPeriod { get; set; } = null;

    /// <summary>
    /// Gets or sets the required date to change the password.
    /// </summary>
    public DateTimeOffset? RequirePasswordChangeAt { get; set; } = null;

    /// <summary>
    /// Gets a value indicating whether it is necessary to change password.
    /// </summary>
    public bool IsPasswordChangeRequired
        => RequirePasswordChangeAt is not null && DateTimeOffset.UtcNow >= RequirePasswordChangeAt;

    /// <summary>
    /// Gets the current authentication version of the user.
    ///
    /// Changing this value invalidates previously issued authentication tokens.
    /// </summary>
    public Guid AuthVersion { get; private set; } = Guid.NewGuid();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private UserAuthentication() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public UserAuthentication(User user)
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
}