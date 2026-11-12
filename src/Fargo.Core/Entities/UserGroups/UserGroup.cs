using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Informations;

namespace Fargo.Core.Entities.UserGroups;

/// <summary>
/// A set of permissions granted to its members, scoped by partitions.
/// </summary>
/// <remarks>
/// A user may access the group only if they have access to at least one of its partitions,
/// subject to additional authorization rules.
///
/// Every group is associated with the global partition, which cannot be removed. The built-in
/// administrators group is restricted to the global partition, cannot be associated with any other
/// one, and has explicit access to it that cannot be revoked.
/// </remarks>
public class UserGroup : Entity, IPartitionedGuidsReadOnly, IConcurrencyVersioned
{
    public bool IsActive { get; private set; } = true;

    public bool IsAdministrators => Guid == FargoCoreWellKnowGuids.AdministratorsUserGroupGuid;

    public Nameid Nameid { get; set; }

    public Description Description { get; set; } = Description.Empty;

    /// <summary>
    /// <see langword="null"/> for a root group.
    /// </summary>
    public Guid? ParentUserGroupGuid { get; private set; }

    public UserGroup? ParentUserGroup { get; private set; }

    public IReadOnlyCollection<ActionType> Permissions => [.. permissionGrants.Select(grant => grant.ActionType)];

    /// <summary>
    /// The persisted rows behind <see cref="Permissions"/>.
    /// </summary>
    public IReadOnlyCollection<UserGroupPermission> PermissionGrants => permissionGrants;

    private readonly List<UserGroupPermission> permissionGrants = [];

    public IReadOnlyCollection<UserGroupPartition> Partitions => partitions;

    public IReadOnlyCollection<Guid> PartitionGuids => [.. partitions.Select(p => p.PartitionGuid)];

    private readonly List<UserGroupPartition> partitions = [];

    /// <summary>
    /// The partitions members are granted direct access to; access to descendant partitions is
    /// inherited through the hierarchy. Distinct from <see cref="Partitions"/>, which scope the group
    /// itself.
    /// </summary>
    public IReadOnlyCollection<UserGroupPartitionAccess> PartitionAccesses => partitionAccesses;

    private readonly List<UserGroupPartitionAccess> partitionAccesses = [];

    // Required by EF Core. Every group is associated with the global partition from creation.
    private UserGroup()
    {
        EntityType = EntityType.UserGroup;

        partitions.Add(new UserGroupPartition(this, FargoCoreWellKnowGuids.GlobalPartitionGuid));
    }

    public static UserGroup CreateUserGroup(Nameid nameid)
    {
        var usergroup = new UserGroup
        {
            Nameid = nameid
        };

        return usergroup;
    }

    /// <summary>
    /// Creates the built-in administrators group, with explicit access to the global partition that
    /// cannot be revoked.
    /// </summary>
    public static UserGroup CreateAdministratorsUserGroup(Nameid nameid)
    {
        var administratorsUsergroup = new UserGroup
        {
            Guid = FargoCoreWellKnowGuids.AdministratorsUserGroupGuid,
            Nameid = nameid
        };

        administratorsUsergroup.AddPartitionAccess(FargoCoreWellKnowGuids.GlobalPartitionGuid);

        return administratorsUsergroup;
    }

    /// <summary>
    /// Does nothing if the access already exists.
    /// </summary>
    public void AddPartitionAccess(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (partitionAccesses.Any(p => p.PartitionGuid == partition.Guid))
        {
            return;
        }

        partitionAccesses.Add(new UserGroupPartitionAccess(this, partition));
    }

    internal void AddPartitionAccess(Guid partitionGuid)
    {
        if (partitionGuid == Guid.Empty)
        {
            throw new ArgumentException(
                "Partition GUID cannot be empty.",
                nameof(partitionGuid));
        }

        if (partitionAccesses.Any(p => p.PartitionGuid == partitionGuid))
        {
            return;
        }

        partitionAccesses.Add(new UserGroupPartitionAccess(this, partitionGuid));
    }

    /// <summary>
    /// Does nothing if the group has no such access.
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// Attempting to revoke the administrators group's access to the global partition.
    /// </exception>
    public void RemovePartitionAccess(Guid partitionGuid)
    {
        if (IsAdministrators && partitionGuid == FargoCoreWellKnowGuids.GlobalPartitionGuid)
        {
            throw new FargoCoreException(
                $"Cannot revoke the administrators user group '{FargoCoreWellKnowGuids.AdministratorsUserGroupGuid}' access to the global partition '{FargoCoreWellKnowGuids.GlobalPartitionGuid}'.",
                FargoErrorType.InvalidOperation);
        }

        partitionAccesses.RemoveAll(p => p.PartitionGuid == partitionGuid);
    }

    /// <summary>
    /// Does nothing if the group is already associated with the partition.
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// Associating the administrators group with a non-global partition.
    /// </exception>
    public void AddPartition(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (IsAdministrators && !partition.IsGlobalPartition)
        {
            throw new FargoCoreException(
                $"Cannot associate the administrators user group '{FargoCoreWellKnowGuids.AdministratorsUserGroupGuid}' with the non-global partition '{partition.Guid}'.",
                FargoErrorType.InvalidOperation);
        }

        if (partitions.Any(p => p.PartitionGuid == partition.Guid))
        {
            return;
        }

        partitions.Add(new UserGroupPartition(this, partition));
    }

    /// <summary>
    /// Does nothing if the group is not associated with the partition.
    /// </summary>
    /// <exception cref="FargoCoreException">The partition is the global partition.</exception>
    public void RemovePartition(Guid partitionGuid)
    {
        if (partitionGuid == FargoCoreWellKnowGuids.GlobalPartitionGuid)
        {
            throw new FargoCoreException(
                $"The global partition '{FargoCoreWellKnowGuids.GlobalPartitionGuid}' is mandatory and cannot be removed from the user group '{Guid}'.",
                FargoErrorType.InvalidOperation);
        }

        partitions.RemoveAll(p => p.PartitionGuid == partitionGuid);
    }

    /// <summary>
    /// Does nothing if the permission is already granted.
    /// </summary>
    public void AddPermission(ActionType action)
    {
        if (permissionGrants.Any(grant => grant.ActionType == action))
        {
            return;
        }

        permissionGrants.Add(new UserGroupPermission(this, action));
    }

    /// <exception cref="FargoCoreException">The group is the administrators group.</exception>
    public void RemovePermission(ActionType action)
    {
        if (IsAdministrators)
        {
            throw new FargoCoreException(
                $"Cannot revoke the permission '{action}' from the administrators user group '{FargoCoreWellKnowGuids.AdministratorsUserGroupGuid}'.",
                FargoErrorType.InvalidOperation);
        }

        permissionGrants.RemoveAll(grant => grant.ActionType == action);
    }

    public void Activate()
    {
        IsActive = true;
    }

    /// <exception cref="FargoCoreException">The group is the administrators group.</exception>
    public void Deactivate()
    {
        if (IsAdministrators)
        {
            throw new FargoCoreException(
                $"Cannot deactivate the administrators user group '{FargoCoreWellKnowGuids.AdministratorsUserGroupGuid}'.",
                FargoErrorType.InvalidOperation);
        }

        IsActive = false;
    }

    /// <summary>
    /// Checks only what this group can tell on its own; the presence of child groups is checked by
    /// <see cref="UserGroupService"/>.
    /// </summary>
    /// <exception cref="FargoCoreException">The group is the built-in administrators group.</exception>
    public void ValidateCanBeDeleted()
    {
        if (IsAdministrators)
        {
            throw new FargoCoreException(
                $"The administrators user group '{Guid}' cannot be deleted.",
                FargoErrorType.InvalidOperation);
        }
    }

    /// <summary>
    /// Does <b>not</b> detect circular hierarchies; validate with
    /// <see cref="UserGroupService"/> first.
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// The group is the administrators group, or <paramref name="parentUserGroup"/> is itself.
    /// </exception>
    public void SetParentUserGroup(UserGroup parentUserGroup)
    {
        ArgumentNullException.ThrowIfNull(parentUserGroup);

        if (IsAdministrators)
        {
            throw new FargoCoreException(
                $"Administrators user group '{FargoCoreWellKnowGuids.AdministratorsUserGroupGuid}' cannot be added inside another user group '{parentUserGroup.Guid}'.",
                FargoErrorType.InvalidOperation);
        }

        if (parentUserGroup.Guid == Guid)
        {
            throw new FargoCoreException(
                $"The user group '{Guid}' cannot be its own parent.",
                FargoErrorType.InvalidOperation);
        }

        ParentUserGroup = parentUserGroup;
        ParentUserGroupGuid = parentUserGroup.Guid;
    }

    public void RemoveFromParentUserGroup()
    {
        ParentUserGroup = null;
        ParentUserGroupGuid = null;
    }
}
