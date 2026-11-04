using Fargo.Application.Common;
using Fargo.Core.Identity.Authentication;
using Fargo.Core.Identity.Tokens;

namespace Fargo.Application.Identity;

/// <summary>
/// Command used to refresh authentication tokens using a valid refresh token.
/// </summary>
/// <param name="RefreshToken">
/// The refresh token provided by the client.
/// </param>
public sealed record IdentityRefreshCommand(Token RefreshToken) : ICommand<IdentityAuthResultDto>;
