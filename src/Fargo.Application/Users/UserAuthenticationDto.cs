namespace Fargo.Application.Users;

public sealed record UserAuthenticationDto(
    TimeSpan? DefaultPasswordExpirationPeriod = null,
    DateTimeOffset? RequirePasswordChangeAt = null
);
