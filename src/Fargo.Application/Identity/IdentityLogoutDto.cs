using Fargo.Core.Identity;

namespace Fargo.Application.Identity;

/// <summary>
/// Represents the data transfer object for logging out by providing a refresh token.
/// This DTO is used as input when performing a logout operation to revoke the refresh token.
/// </summary>
/// <param name="RefreshToken">The refresh token to be revoked during logout</param>
public sealed record IdentityLogOutDto(Token RefreshToken);
