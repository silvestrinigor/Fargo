using Fargo.Core.Shared.Identity;

namespace Fargo.Application.Identity.Commands;

/// <summary>
/// Command used to log out a user by invalidating a refresh token.
/// </summary>
/// <param name="RefreshToken">
/// The refresh token provided by the client.
/// </param>
public sealed record LogoutCommand(Token RefreshToken) : ICommand;
