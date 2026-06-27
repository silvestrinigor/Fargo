using Fargo.Application.Shared.Identity;

namespace Fargo.Application.Identity;

/// <summary>
/// Command used to authenticate a user with a nameid and password.
/// </summary>
/// <param name="Nameid">
/// The unique user identifier used for login.
/// </param>
/// <param name="Password">
/// The plaintext password provided for authentication.
/// </param>
public sealed record LoginCommand(
    string Nameid,
    string Password
) : ICommand<AuthResult>;
