namespace Fargo.Application.Shared.Users;

public sealed record UserAuthenticationDto(
    TimeSpan? DefaultPasswordExpirationPeriod = null,
    DateTimeOffset? RequirePasswordChangeAt = null
);
