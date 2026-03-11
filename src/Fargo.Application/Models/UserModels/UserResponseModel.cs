using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserModels
{
    /// <summary>
    /// Represents the user data returned by the application.
    /// </summary>
    /// <param name="Guid">
    /// The unique identifier of the user.
    /// </param>
    /// <param name="Nameid">
    /// The login identifier of the user.
    /// </param>
    /// <param name="FirstName">
    /// The first name of the user, if available.
    /// </param>
    /// <param name="LastName">
    /// The last name of the user, if available.
    /// </param>
    /// <param name="DefaultPasswordExpirationPeriod">
    /// The default password expiration period configured for the user.
    /// This value represents the duration applied when the user changes their own password.
    /// </param>
    /// <param name="Description">
    /// The description associated with the user.
    /// </param>
    /// <param name="IsActive">
    /// Indicates whether the user account is currently active.
    ///
    /// An inactive user cannot authenticate or perform any authorized
    /// operations within the system.
    /// </param>
    /// <param name="Permissions">
    /// The permissions granted to the user.
    /// </param>
    public sealed record UserResponseModel(
            Guid Guid,
            Nameid Nameid,
            FirstName? FirstName,
            LastName? LastName,
            TimeSpan DefaultPasswordExpirationPeriod,
            Description Description,
            bool IsActive,
            IReadOnlyCollection<UserPermissionResponseModel> Permissions
            );
}