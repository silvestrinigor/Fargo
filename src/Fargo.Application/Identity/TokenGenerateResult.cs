using Fargo.Core.Shared.Identity;
using Fargo.Core.Users;

namespace Fargo.Application.Identity;

/// <summary>
/// Represents the result of generating an access token.
/// </summary>
/// <param name="AccessToken">
/// The generated access token used to authenticate requests.
/// </param>
/// <param name="ExpiresAt">
/// The date and time when the access token expires.
/// </param>
public record TokenGenerateResult(
    Token AccessToken,
    DateTimeOffset ExpiresAt);
