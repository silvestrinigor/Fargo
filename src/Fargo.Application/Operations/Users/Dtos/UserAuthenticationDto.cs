namespace Fargo.Application.Operations.Users.Dtos;

public sealed record UserAuthenticationDto(
    TimeSpan? DefaultPasswordExpirationPeriod = null,
    DateTimeOffset? RequirePasswordChangeAt = null
);
