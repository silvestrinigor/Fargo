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
    /// <param name="CreatedAt">
    /// The date and time when the user was created.
    /// </param>
    /// <param name="CreatedByGuid">
    /// The unique identifier of the user that created this user record.
    ///
    /// This value may be <c>null</c> when the user was created by a
    /// system process or when the creator information is not available.
    /// </param>
    /// <param name="EditedAt">
    /// The date and time when the user was last modified.
    ///
    /// This value may be <c>null</c> when the user has not been modified
    /// since creation.
    /// </param>
    /// <param name="EditedByGuid">
    /// The unique identifier of the user that last modified this user record.
    ///
    /// This value may be <c>null</c> when the user has not been modified
    /// since creation.
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
        DateTimeOffset CreatedAt,
        Guid? CreatedByGuid,
        DateTimeOffset? EditedAt,
        Guid? EditedByGuid,
        IReadOnlyCollection<UserPermissionResponseModel> Permissions
    );
}