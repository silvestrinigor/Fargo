using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.UserGroups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Fargo.Infrastructure.Persistence;

/// <summary>
/// Renews the <see cref="AuthorizationStamp"/> in the same save whenever an entity that affects
/// permissions or partition access is added, modified or deleted, so cached authorization snapshots
/// are invalidated in the same transaction as the change.
/// </summary>
/// <remarks>
/// <para>
/// Watched: <see cref="Actor"/> (active flag), <see cref="ActorPermission"/>, <see cref="ActorPartitionAccess"/>,
/// <see cref="UserUserGroup"/> (group membership), <see cref="UserGroup"/> (active flag, parent),
/// <see cref="UserGroupPermission"/>, <see cref="UserGroupPartitionAccess"/> and <see cref="Partition"/> (the tree accesses are
/// expanded along). Any of them invalidates every actor's snapshot, which is simpler than working out
/// who is affected and cheap because such changes are rare.
/// </para>
/// <para>
/// Changes made outside the change tracker (raw SQL, <c>ExecuteUpdate</c>/<c>ExecuteDelete</c>) are
/// not seen; renew the stamp explicitly in those code paths.
/// </para>
/// <para>
/// Every renewal updates the same row, so concurrent authorization changes are serialized on its lock
/// until they commit.
/// </para>
/// </remarks>
public sealed class AuthorizationStampSaveChangesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        RenewIfNeeded(eventData.Context);

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        RenewIfNeeded(eventData.Context);

        return ValueTask.FromResult(result);
    }

    internal static bool AffectsAuthorization(EntityEntry entry)
        => (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
           && (entry.Entity is Actor
               or ActorPermission
               or UserGroupPermission
               or ActorPartitionAccess
               or UserUserGroup
               or UserGroup
               or UserGroupPartitionAccess
               or Partition);

    private static void RenewIfNeeded(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        // SaveChanges detects changes after the interceptors run.
        context.ChangeTracker.DetectChanges();

        if (!context.ChangeTracker.Entries().Any(AffectsAuthorization))
        {
            return;
        }

        var tracked = context.ChangeTracker.Entries<AuthorizationStamp>().FirstOrDefault();

        if (tracked is not null)
        {
            // A retried or second save in the same context: renew again so the value still changes.
            tracked.Entity.Renew();
            tracked.Property(s => s.Value).IsModified = true;

            return;
        }

        // Update the seeded row without reading it first.
        var stamp = new AuthorizationStamp(Guid.NewGuid());
        var entry = context.Attach(stamp);
        entry.Property(s => s.Value).IsModified = true;
    }
}
