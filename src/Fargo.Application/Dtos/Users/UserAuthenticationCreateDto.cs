using Fargo.Core.Identity.Credentials;

namespace Fargo.Application.Dtos.Users;

public sealed record UserAuthenticationCreateDto(
    Password? Password = null,
    TimeSpan? DefaultPasswordExpirationPeriod = null
);
