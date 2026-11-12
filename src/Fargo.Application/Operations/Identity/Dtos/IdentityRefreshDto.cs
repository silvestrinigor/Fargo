using Fargo.Core.Identity.Tokens;

namespace Fargo.Application.Operations.Identity.Dtos;

/// <summary>
/// Represents the data transfer object for refreshing authentication tokens.
/// This DTO is used as input when requesting a new access token using a refresh token.
/// </summary>
/// <param name="RefreshToken">The refresh token used to obtain a new access token</param>
public sealed record IdentityRefreshDto(Token RefreshToken);
