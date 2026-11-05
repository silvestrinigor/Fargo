using Fargo.Core.Identity.Credentials;

namespace Fargo.Application.Operations.Users.Dtos;

public sealed record UserAuthenticationUpdateDto(
    Password? Password = null,
    TimeSpan? DefaultPasswordExpirationPeriod = null,
    bool? RemoveDefaultPasswordExpirationPeriod = null
);
