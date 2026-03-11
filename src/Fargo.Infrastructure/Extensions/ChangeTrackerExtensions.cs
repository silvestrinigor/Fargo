using Fargo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Fargo.Infrastructure.Extensions
{
    /// <summary>
    /// Provides extension methods for applying auditing rules
    /// to tracked entities.
    /// </summary>
    internal static class ChangeTrackerExtensions
    {
        /// <summary>
        /// Applies modification auditing to tracked entities.
        /// </summary>
        /// <param name="changeTracker">
        /// The EF Core change tracker.
        /// </param>
        /// <param name="currentUserGuid">
        /// The unique identifier of the user performing the operation.
        /// </param>
        /// <remarks>
        /// This method applies auditing in two scenarios:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// When a tracked entity implementing <see cref="IAuditedEntity"/> is in the
        /// <see cref="EntityState.Modified"/> state, its own audit metadata is updated.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// When a tracked entity implementing <see cref="IAuditPropagator"/> is in the
        /// <see cref="EntityState.Added"/>, <see cref="EntityState.Modified"/>,
        /// or <see cref="EntityState.Deleted"/> state, the audit metadata of its
        /// parent audited entity is updated.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        public static void ApplyAuditing(this ChangeTracker changeTracker, Guid currentUserGuid)
        {
            foreach (var entry in changeTracker.Entries<IAuditedEntity>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.MarkAsEdited(currentUserGuid);
                }
            }

            var parentEntitiesToUpdate = changeTracker
                .Entries<IAuditedAggregateMember>()
                .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .Select(entry => entry.Entity.ParentAuditedEntity)
                .Distinct()
                .ToList();

            foreach (var parentEntity in parentEntitiesToUpdate)
            {
                parentEntity.MarkAsEdited(currentUserGuid);
            }
        }
    }
}