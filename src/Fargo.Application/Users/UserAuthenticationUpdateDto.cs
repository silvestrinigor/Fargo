using Fargo.Core.Security;

namespace Fargo.Application.Users;

public sealed record UserAuthenticationUpdateDto(
    Password? Password = null,
    TimeSpan? DefaultPasswordExpirationPeriod = null,
    bool? RemoveDefaultPasswordExpirationPeriod = null
);
