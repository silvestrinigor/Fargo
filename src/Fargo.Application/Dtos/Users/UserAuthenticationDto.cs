namespace Fargo.Application.Dtos.Users;

public sealed record UserAuthenticationDto(
    TimeSpan? DefaultPasswordExpirationPeriod = null,
    DateTimeOffset? RequirePasswordChangeAt = null
);
