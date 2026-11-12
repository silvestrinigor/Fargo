using Fargo.Core.Identity.Credentials;

namespace Fargo.Core.Entities.Actors.Users;

public class UserAuthentication
{
    public Guid UserGuid { get; private init; }

    public User User { get; private init; }

    public PasswordHash? PasswordHash { get; private set; } = null;

    /// <summary>
    /// <see langword="null"/> means passwords do not expire.
    /// </summary>
    public TimeSpan? DefaultPasswordExpirationPeriod { get; set; } = null;

    /// <summary>
    /// Instant from which the user must change their password; <see langword="null"/> when no change
    /// is required.
    /// </summary>
    public DateTimeOffset? RequirePasswordChangeAt { get; set; } = null;

    public bool IsPasswordChangeRequiredAt(DateTimeOffset now)
        => RequirePasswordChangeAt is not null && now >= RequirePasswordChangeAt;

    /// <summary>
    /// Changing this value invalidates all previously issued authentication tokens.
    /// </summary>
    public Guid AuthVersion { get; private set; } = Guid.NewGuid();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private UserAuthentication() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    internal UserAuthentication(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        User = user;
        UserGuid = user.ActorGuid;
    }

    /// <summary>
    /// Sets the expiration to <paramref name="now"/> plus <see cref="DefaultPasswordExpirationPeriod"/>
    /// (immediately when it is <see cref="TimeSpan.Zero"/>, never when it is <see langword="null"/>).
    /// Typically called after the user changes their own password.
    /// </summary>
    public void ResetPasswordExpiration(DateTimeOffset now)
        => RequirePasswordChangeAt = now + DefaultPasswordExpirationPeriod;

    /// <summary>
    /// Requires a change from <paramref name="now"/> until the expiration is reset.
    /// </summary>
    public void MarkPasswordChangeAsRequired(DateTimeOffset now)
    {
        RequirePasswordChangeAt = now;
    }

    /// <summary>
    /// Invalidates all previously issued authentication tokens.
    /// </summary>
    public void RotateAuthVersion()
    {
        AuthVersion = Guid.NewGuid();
    }

    public void SetPasswordHash(PasswordHash passwordHash)
    {
        PasswordHash = passwordHash;
    }
}
