using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserModels
{
    /// <summary>
    /// Represents the data required to update a user's password.
    /// </summary>
    /// <param name="CurrentPassword">
    /// The user's current password used to verify the request.
    /// </param>
    /// <param name="NewPassword">
    /// The new password that will replace the current one.
    /// </param>
    public sealed record UserPasswordUpdateModel(
            Password CurrentPassword,
            Password NewPassword
            );
}