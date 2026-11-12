using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.UserGroups;

namespace Fargo.Application.Common.Actors;

/// <summary>
/// Shared rules for the dedicated endpoints that change who belongs where and who can reach what:
/// user-group membership, partitions and user-group parents.
/// </summary>
public static class ActorScopeChangeExtensions
{
    /// <summary>
    /// Removes duplicates and rejects an item that is both added and removed, which would silently
    /// let the removal win.
    /// </summary>
    public static (IReadOnlyList<T> ToAdd, IReadOnlyList<T> ToRemove) DistinctChange<T>(
        IReadOnlyCollection<T>? toAdd, IReadOnlyCollection<T>? toRemove, string what)
    {
        IReadOnlyList<T> add = [.. (toAdd ?? []).Distinct()];
        IReadOnlyList<T> remove = [.. (toRemove ?? []).Distinct()];

        var conflicting = add.Intersect(remove).ToList();

        if (conflicting.Count > 0)
        {
            throw new FargoApplicationException(
                $"The {what} {string.Join(", ", conflicting)} cannot be added and removed in the same request.",
                FargoErrorType.InvalidOperation);
        }

        return (add, remove);
    }

    public static (IReadOnlyList<Guid> ToAdd, IReadOnlyList<Guid> ToRemove) ValidateUserGroupsChange(UserGroupsChangeDto change)
    {
        ArgumentNullException.ThrowIfNull(change);

        var (toAdd, toRemove) = DistinctChange(change.UserGroupsToAdd, change.UserGroupsToRemove, "user group(s)");

        if (toAdd.Count == 0 && toRemove.Count == 0)
        {
            throw new FargoApplicationException(
                "Specify at least one user group to add or remove.",
                FargoErrorType.InvalidOperation);
        }

        return (toAdd, toRemove);
    }

    /// <summary>
    /// A user inherits the permissions of its groups and of their ancestors, so joining a group, or
    /// placing a group under a parent, hands those permissions to the members. The actor must hold
    /// every one of them itself, otherwise it could give out more than it has.
    /// </summary>
    /// <remarks>
    /// The whole chain is read in one query. Inactive groups in the chain are counted too: an inactive
    /// group can be activated later and would then pass its permissions on.
    /// </remarks>
    public static async Task ThrowIfMissingInheritedPermissionsAsync(
        this SecurityPrincipal actor,
        UserGroup userGroup,
        IUserGroupRepository userGroupRepository,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userGroup);

        var permissions = await userGroupRepository.GetInheritedPermissionsAsync(userGroup.Guid, cancellationToken);

        foreach (var permission in permissions)
        {
            actor.ThrowIfPermissionDenied(permission);
        }
    }
}
/// <summary>
/// The validated, de-duplicated form of a <see cref="PartitionsChangeDto"/>.
/// </summary>
public sealed record PartitionsChangePlan(
    IReadOnlyList<Guid> MembershipsToAdd,
    IReadOnlyList<Guid> MembershipsToRemove,
    IReadOnlyList<Guid> AccessesToAdd,
    IReadOnlyList<Guid> AccessesToRemove)
{
    /// <summary>
    /// Runs before the target is loaded so a malformed request fails without touching the database.
    /// </summary>
    public static PartitionsChangePlan From(PartitionsChangeDto change)
    {
        ArgumentNullException.ThrowIfNull(change);

        var (membershipsToAdd, membershipsToRemove) = ActorScopeChangeExtensions.DistinctChange(
            change.PartitionsToAdd, change.PartitionsToRemove, "partition membership(s)");

        var (accessesToAdd, accessesToRemove) = ActorScopeChangeExtensions.DistinctChange(
            change.PartitionAccessesToAdd, change.PartitionAccessesToRemove, "partition access(es)");

        if (membershipsToAdd.Count + membershipsToRemove.Count + accessesToAdd.Count + accessesToRemove.Count == 0)
        {
            throw new FargoApplicationException(
                "Specify at least one partition to add or remove.",
                FargoErrorType.InvalidOperation);
        }

        return new PartitionsChangePlan(membershipsToAdd, membershipsToRemove, accessesToAdd, accessesToRemove);
    }

    /// <remarks>
    /// The actor needs access to every partition it touches, for removals as well as additions.
    /// Memberships are applied first, then accesses.
    /// </remarks>
    public async Task ApplyAsync(
        SecurityPrincipal actor,
        IPartitionRepository partitionRepository,
        Action<Partition> addMembership,
        Action<Guid> removeMembership,
        Action<Partition> addAccess,
        Action<Guid> removeAccess,
        CancellationToken cancellationToken = default)
    {
        foreach (var guid in MembershipsToAdd)
        {
            addMembership(await ResolveAsync(actor, partitionRepository, guid, cancellationToken));
        }

        foreach (var guid in MembershipsToRemove)
        {
            removeMembership((await ResolveAsync(actor, partitionRepository, guid, cancellationToken)).Guid);
        }

        foreach (var guid in AccessesToAdd)
        {
            addAccess(await ResolveAsync(actor, partitionRepository, guid, cancellationToken));
        }

        foreach (var guid in AccessesToRemove)
        {
            removeAccess((await ResolveAsync(actor, partitionRepository, guid, cancellationToken)).Guid);
        }
    }

    private static async Task<Partition> ResolveAsync(
        SecurityPrincipal actor, IPartitionRepository partitionRepository, Guid partitionGuid, CancellationToken cancellationToken)
    {
        var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

        actor.ThrowIfAccessDenied(partition);

        return partition;
    }
}
