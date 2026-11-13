using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Common.Actors;

public static class ActorPermissionChangeExtensions
{
    /// <remarks>
    /// Grants and revocations are checked one by one: the actor must hold every permission it
    /// changes, so it can neither hand out nor take away what it does not have itself.
    /// </remarks>
    public static void ApplyPermissionsChange(
        this SecurityPrincipal actor,
        IReadOnlyList<ActionType> toAdd,
        IReadOnlyList<ActionType> toRemove,
        Action<ActionType> grant,
        Action<ActionType> revoke)
    {
        foreach (var permission in toAdd)
        {
            actor.ThrowIfPermissionDenied(permission);

            grant(permission);
        }

        foreach (var permission in toRemove)
        {
            actor.ThrowIfPermissionDenied(permission);

            revoke(permission);
        }
    }

    /// <summary>
    /// An actor cannot change its own permissions, so it can neither lock itself out nor widen its own access.
    /// </summary>
    public static void ThrowIfChangingOwnPermissions(this SecurityPrincipal actor, Guid targetActorGuid, ActorType targetActorType)
    {
        if (actor.Guid == targetActorGuid && actor.ActorType == targetActorType)
        {
            throw new FargoApplicationException(
                $"The actor '{actor.Guid}' cannot change its own permissions.",
                FargoErrorType.InvalidOperation);
        }
    }

    /// <summary>
    /// Runs before the target is loaded so a malformed request fails without touching the database.
    /// </summary>
    public static (IReadOnlyList<ActionType> ToAdd, IReadOnlyList<ActionType> ToRemove) ValidatePermissionsChange(
        PermissionsChangeDto change)
    {
        ArgumentNullException.ThrowIfNull(change);

        IReadOnlyList<ActionType> toAdd = [.. (change.PermissionsToAdd ?? []).Distinct()];
        IReadOnlyList<ActionType> toRemove = [.. (change.PermissionsToRemove ?? []).Distinct()];

        if (toAdd.Count == 0 && toRemove.Count == 0)
        {
            throw new FargoApplicationException(
                "Specify at least one permission to add or remove.",
                FargoErrorType.InvalidOperation);
        }

        var conflicting = toAdd.Intersect(toRemove).ToList();

        if (conflicting.Count > 0)
        {
            throw new FargoApplicationException(
                $"The permission(s) {string.Join(", ", conflicting)} cannot be added and removed in the same request.",
                FargoErrorType.InvalidOperation);
        }

        return (toAdd, toRemove);
    }
}
