using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserGroupModels
{
    /// <summary>
    /// Represents the response model returned when querying a user group.
    /// </summary>
    /// <remarks>
    /// This model is returned to API consumers and contains the user group
    /// information, auditing metadata, and its assigned permissions.
    /// </remarks>
    /// <param name="Guid">
    /// The unique identifier of the user group.
    /// </param>
    /// <param name="Nameid">
    /// The login identifier (NAMEID) of the user group.
    /// </param>
    /// <param name="Description">
    /// The description associated with the user group.
    /// </param>
    /// <param name="IsActive">
    /// Indicates whether the user group is active.
    /// </param>
    /// <param name="CreatedAt">
    /// The date and time when the user group was created.
    /// </param>
    /// <param name="CreatedByGuid">
    /// The unique identifier of the user that created this user group.
    ///
    /// This value may be <c>null</c> when the user group was created by a
    /// system process or when the creator information is not available.
    /// </param>
    /// <param name="EditedAt">
    /// The date and time when the user group was last modified.
    ///
    /// This value may be <c>null</c> when the user group has not been modified
    /// since creation.
    /// </param>
    /// <param name="EditedByGuid">
    /// The unique identifier of the user that last modified this user group.
    ///
    /// This value may be <c>null</c> when the user group has not been modified
    /// since creation.
    /// </param>
    /// <param name="UserGroupPermissions">
    /// The permissions assigned to the user group.
    /// </param>
    public sealed record UserGroupResponseModel(
        Guid Guid,
        Nameid Nameid,
        Description Description,
        bool IsActive,
        DateTimeOffset CreatedAt,
        Guid? CreatedByGuid,
        DateTimeOffset? EditedAt,
        Guid? EditedByGuid,
        IReadOnlyCollection<UserGroupPermissionResponseModel> UserGroupPermissions
    );
}