using Fargo.Domain;

namespace Fargo.Application.Models.UserGroupModels;

/// <summary>
/// Represents the data used to update an existing user group.
/// </summary>
/// <remarks>
/// All properties are optional. Only the values explicitly provided
/// will be applied to the target user group.
/// </remarks>
/// <param name="Nameid">
/// The new identifier of the user group.
/// If <see langword="null"/>, the current value is preserved.
/// </param>
/// <param name="Description">
/// The new description associated with the user group.
/// If <see langword="null"/>, the current value is preserved.
/// </param>
/// <param name="IsActive">
/// Indicates whether the user group should be active.
/// If <see langword="null"/>, the current activation state is preserved.
/// </param>
/// <param name="Permissions">
/// The complete set of permissions to assign to the user group.
///
/// When provided, the group's permissions are synchronized with this
/// collection: permissions not present are removed and new permissions
/// are added.
///
/// If <see langword="null"/>, the existing permissions are preserved.
/// </param>
public sealed record UserGroupUpdateModel(
        string? Nameid = null,
        Description? Description = null,
        bool? IsActive = null,
        IReadOnlyCollection<UserGroupPermissionUpdateModel>? Permissions = null
        );
