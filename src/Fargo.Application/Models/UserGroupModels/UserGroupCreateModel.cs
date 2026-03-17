using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserGroupModels;

/// <summary>
/// Represents the data required to create a new user group.
/// </summary>
/// <remarks>
/// A user group aggregates a set of permissions that can be assigned
/// to multiple users. This model defines the information necessary
/// to create the group and optionally configure its initial permissions.
/// </remarks>
/// <param name="Nameid">
/// The unique identifier of the user group.
/// This value must satisfy the validation rules defined in <see cref="Nameid"/>.
/// </param>
/// <param name="Description">
/// The optional description associated with the user group.
/// If <see langword="null"/>, the group will be created without a description.
/// </param>
/// <param name="Permissions">
/// The collection of permissions to assign to the group during creation.
/// If <see langword="null"/>, the group will be created without permissions.
/// </param>
public sealed record UserGroupCreateModel(
        Nameid Nameid,
        Description? Description = null,
        IReadOnlyCollection<UserGroupPermissionUpdateModel>? Permissions = null
        );
