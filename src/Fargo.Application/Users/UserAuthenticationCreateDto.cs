using Fargo.Core.Shared.Security;

namespace Fargo.Application.Shared.Users;

public sealed record UserAuthenticationCreateDto(
    Password? Password = null,
    TimeSpan? DefaultPasswordExpirationPeriod = null
);
