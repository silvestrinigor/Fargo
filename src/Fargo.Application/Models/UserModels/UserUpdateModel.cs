using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserModels
{
    /// <summary>
    /// Represents the data used to update an existing user.
    /// </summary>
    /// <remarks>
    /// All properties are optional. Only the values explicitly provided
    /// will be applied to the target user.
    /// </remarks>
    /// <param name="Nameid">
    /// The new login identifier of the user.
    /// If <see langword="null"/>, the current value is preserved.
    /// </param>
    /// <param name="FirstName">
    /// The new first name of the user.
    /// If <see langword="null"/>, the current value is preserved.
    /// </param>
    /// <param name="LastName">
    /// The new last name of the user.
    /// If <see langword="null"/>, the current value is preserved.
    /// </param>
    /// <param name="Description">
    /// The new description associated with the user.
    /// If <see langword="null"/>, the current value is preserved.
    /// </param>
    /// <param name="Password">
    /// The new password to assign to the user.
    /// This field is intended for administrative password changes only.
    /// If <see langword="null"/>, the password remains unchanged.
    /// </param>
    /// <param name="Permissions">
    /// The complete set of permissions to assign to the user.
    ///
    /// When provided, the user's permissions are synchronized with this collection:
    /// permissions not present are removed and new permissions are added.
    ///
    /// If <see langword="null"/>, the existing permissions are preserved.
    /// </param>
    /// <param name="DefaultPasswordExpirationTimeSpan">
    /// The new default password expiration interval for the user.
    ///
    /// This value defines how long a password remains valid after the user
    /// successfully changes it.
    ///
    /// If <see langword="null"/>, the current value is preserved.
    ///
    /// A value of <see cref="TimeSpan.Zero"/> causes the password to expire
    /// immediately after it is changed.
    /// </param>
    public sealed record UserUpdateModel(
            Nameid? Nameid = null,
            FirstName? FirstName = null,
            LastName? LastName = null,
            Description? Description = null,
            Password? Password = null,
            IReadOnlyCollection<UserPermissionUpdateModel>? Permissions = null,
            TimeSpan? DefaultPasswordExpirationTimeSpan = null
            );
}