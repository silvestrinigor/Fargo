using Fargo.Core.Security;

namespace Fargo.Application.Users;

public sealed record UserAuthenticationCreateDto(
    Password? Password = null,
    TimeSpan? DefaultPasswordExpirationPeriod = null
);
