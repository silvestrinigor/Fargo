using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserModels
{
    /// <summary>
    /// Represents the data used to update an existing user.
    /// </summary>
    /// <remarks>
    /// All properties are optional. Only the provided values will be updated.
    /// </remarks>
    /// <param name="Nameid">
    /// The new login identifier of the user. If null, the nameid will not be changed.
    /// </param>
    /// <param name="Description">
    /// The new description of the user. If null, the description will not be changed.
    /// </param>
    /// <param name="Password">
    /// The password update information containing the current password and the new password.
    /// If null, the password will not be changed.
    /// </param>
    public sealed record UserUpdateModel(
            Nameid? Nameid = null,
            Description? Description = null,
            UserPasswordUpdateModel? Password = null
            );
}