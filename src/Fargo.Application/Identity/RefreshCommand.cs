using Fargo.Application.Shared.Identity;
using Fargo.Core.Shared.Identity;

namespace Fargo.Application.Identity;

/// <summary>
/// Command used to refresh authentication tokens using a valid refresh token.
/// </summary>
/// <param name="RefreshToken">
/// The refresh token provided by the client.
/// </param>
public sealed record RefreshCommand(Token RefreshToken) : ICommand<AuthResult>;
