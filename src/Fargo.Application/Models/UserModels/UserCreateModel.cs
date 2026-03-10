using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserModels
{
    /// <summary>
    /// Represents the data required to create a new user.
    /// </summary>
    /// <param name="Nameid">
    /// The unique login identifier of the user.
    /// This value must satisfy the validation rules defined in <see cref="Nameid"/>.
    /// </param>
    /// <param name="Password">
    /// The plaintext password that will be hashed before being stored.
    /// </param>
    /// <param name="FirstName">
    /// Optional first name of the user.
    /// When provided, the value must satisfy the validation rules defined in
    /// <see cref="FirstName"/>.
    /// </param>
    /// <param name="LastName">
    /// Optional last name of the user.
    /// When provided, the value must satisfy the validation rules defined in
    /// <see cref="LastName"/>.
    /// </param>
    /// <param name="Description">
    /// Optional textual description associated with the user.
    /// If not provided, the default value defined by the domain will be used.
    /// </param>
    /// <param name="Permissions">
    /// Optional list of permissions granted to the user.
    /// Each permission defines an action the user is allowed to perform.
    /// If not provided, the user will be created without additional permissions.
    /// </param>
    /// <param name="DefaultPasswordExpirationTimeSpan">
    /// Optional password expiration interval configured for the user.
    ///
    /// When the user successfully changes their password, this value is added
    /// to the current UTC time to determine when the next password change
    /// will be required.
    ///
    /// If <see langword="null"/>, the system default expiration period defined
    /// by the domain will be used.
    ///
    /// A value of <see cref="TimeSpan.Zero"/> causes the password to expire
    /// immediately after it is changed.
    /// </param>
    public record UserCreateModel(
            Nameid Nameid,
            Password Password,
            FirstName? FirstName = null,
            LastName? LastName = null,
            Description? Description = null,
            IReadOnlyCollection<UserPermissionUpdateModel>? Permissions = null,
            TimeSpan? DefaultPasswordExpirationTimeSpan = null
            );
}