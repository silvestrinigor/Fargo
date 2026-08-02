using Fargo.Core.Shared;

namespace Fargo.Application.Shared.Users;

public sealed record UserAuthenticationUpdateDto(
    Password? Password = null,
    TimeSpan? DefaultPasswordExpirationPeriod = null,
    bool? RemoveDefaultPasswordExpirationPeriod = null
);
