using Fargo.Core.Security;

namespace Fargo.Application.Identity;

/// <summary>
/// Represents the data transfer object for updating a user's password.
/// The NewPassword field uses the Password structure to enforce security validation rules
/// including minimum character requirements, special character constraints, and other password policies.
/// </summary>
/// <param name="Nameid">The unique identifier of the user whose password is being changed</param>
/// <param name="NewPassword">The new password value that will be validated against security rules</param>
/// <param name="CurrentPassword">The user's current password for verification purposes</param>
public sealed record IdentityPasswordUpdateDto(
    string Nameid,
    Password NewPassword,
    string CurrentPassword
);
