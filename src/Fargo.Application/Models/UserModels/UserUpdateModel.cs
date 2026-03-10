using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserModels
{
    /// <summary>
    /// Represents the data used to update an existing user by an administrative operation.
    /// </summary>
    /// <remarks>
    /// All properties are optional. Only the provided values will be updated.
    /// </remarks>
    /// <param name="Nameid">
    /// The new login identifier of the user. If <c>null</c>, the nameid will not be changed.
    /// </param>
    /// <param name="Description">
    /// The new description of the user. If <c>null</c>, the description will not be changed.
    /// </param>
    /// <param name="Password">
    /// The new password to assign to the user.
    /// This field is intended for administrative password changes only.
    /// If <c>null</c>, the password will not be changed.
    /// </param>
    /// <param name="Permissions">
    /// The set of permissions to assign to the user.
    /// If <c>null</c>, the user's permissions will not be changed.
    /// </param>
    /// <param name="DefaultPasswordExpirationTimeSpan">
    /// Optional default password expiration period for the user.
    ///
    /// This value defines how long a password remains valid after the user successfully
    /// changes it. The expiration time is calculated by adding this value to the
    /// current UTC time at the moment of the password change.
    ///
    /// If <c>null</c>, the system default expiration policy will be applied.
    ///
    /// A value of <see cref="TimeSpan.Zero"/> causes the password to expire
    /// immediately after it is changed.
    /// </param>
    public sealed record UserUpdateModel(
            Nameid? Nameid = null,
            Description? Description = null,
            Password? Password = null,
            IReadOnlyCollection<UserPermissionUpdateModel>? Permissions = null,
            TimeSpan? DefaultPasswordExpirationTimeSpan = null
            );
}