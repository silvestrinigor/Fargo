using Fargo.Core.Security;

namespace Fargo.Application.Users;

/// <summary>
/// Represents the data required to create authentication information for a user.
/// </summary>
/// <param name="Password">
/// The initial password for the user's authentication credentials.
/// </param>
/// <param name="DefaultPasswordExpirationPeriod">
/// The period after which the user's password expires.
/// </param>
public sealed record UserAuthenticationCreateDto(
    Password? Password = null,
    TimeSpan? DefaultPasswordExpirationPeriod = null
);
