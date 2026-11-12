using Fargo.Core.Identity.Credentials;

namespace Fargo.Application.Dtos.Users;

public sealed record UserAuthenticationUpdateDto(
    Password? Password = null,
    TimeSpan? DefaultPasswordExpirationPeriod = null,
    bool? RemoveDefaultPasswordExpirationPeriod = null
);
