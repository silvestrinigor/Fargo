namespace Fargo.Application.Users;

/// <summary>
/// Represents user authentication information.
/// </summary>
/// <param name="DefaultPasswordExpirationPeriod">
/// The default period after which the user's password expires.
/// </param>
/// <param name="RequirePasswordChangeAt">
/// The date and time at which the user is required to change their password.
/// </param>
public sealed record UserAuthenticationDto(
    TimeSpan? DefaultPasswordExpirationPeriod = null,
    DateTimeOffset? RequirePasswordChangeAt = null
);
