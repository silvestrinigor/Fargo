using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserModels
{
    /// <summary>
    /// Represents the data required to create a new user.
    /// </summary>
    /// <param name="Nameid">
    /// The unique login identifier of the user.
    /// This value must follow the validation rules defined in <see cref="Nameid"/>.
    /// </param>
    /// <param name="Password">
    /// The plaintext password that will be hashed before being stored.
    /// The hashing process is handled by the application's password hashing service.
    /// </param>
    /// <param name="Description">
    /// Optional description of the user.
    /// If not provided, the user will receive the default value defined by the domain.
    /// </param>
    /// <param name="Permissions">
    /// Optional list of permissions granted to the user.
    /// Each permission defines an action that the user is allowed to perform in the system.
    /// If not provided, the user will be created without additional permissions.
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
    public record UserCreateModel(
            Nameid Nameid,
            Password Password,
            Description? Description = null,
            IReadOnlyCollection<UserPermissionUpdateModel>? Permissions = null,
            TimeSpan? DefaultPasswordExpirationTimeSpan = null
            );
}