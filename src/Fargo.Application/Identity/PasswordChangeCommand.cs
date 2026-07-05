using Fargo.Application.Shared.Identity;

namespace Fargo.Application.Identity;

/// <summary>
/// Command used by an authenticated user to change their own password.
/// </summary>
/// <param name="Passwords">
/// The current password and the new password.
/// </param>
public sealed record PasswordChangeCommand(PasswordUpdateDto Passwords) : ICommand;
