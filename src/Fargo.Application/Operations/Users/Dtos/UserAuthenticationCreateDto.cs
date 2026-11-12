using Fargo.Core.Identity.Credentials;

namespace Fargo.Application.Operations.Users.Dtos;

public sealed record UserAuthenticationCreateDto(
    Password? Password = null,
    TimeSpan? DefaultPasswordExpirationPeriod = null
);
