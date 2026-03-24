using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserGroupModels;

/// <summary>
/// Represents the data required to create a new user group.
/// </summary>
/// <remarks>
/// A user group aggregates a set of permissions that can be assigned
/// to multiple users. This model defines the information necessary
/// to create the group and optionally configure its initial permissions.
///
/// User groups are partitioned entities and may belong to one or more partitions.
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
/// Each permission defines an action that will be granted to users belonging
/// to this group.
///
/// If <see langword="null"/>, the group will be created without permissions.
/// </param>
/// <param name="FirstPartition">
/// Optional identifier of the first partition to associate with the user group
/// during creation.
/// </param>
/// <remarks>
/// When <paramref name="FirstPartition"/> is provided, the group is initially
/// associated with that partition. Otherwise, the default partition behavior
/// defined by the application or domain is applied.
/// </remarks>
public sealed record UserGroupCreateModel(
        Nameid Nameid,
        Description? Description = null,
        IReadOnlyCollection<UserGroupPermissionUpdateModel>? Permissions = null,
        Guid? FirstPartition = null
        );
