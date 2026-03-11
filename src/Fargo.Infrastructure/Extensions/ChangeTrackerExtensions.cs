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
        /// Applies modification auditing to tracked auditable entities.
        /// </summary>
        /// <param name="changeTracker">
        /// The EF Core change tracker.
        /// </param>
        /// <param name="currentUserGuid">
        /// The unique identifier of the user performing the operation.
        /// </param>
        /// <remarks>
        /// For each tracked entity implementing <see cref="IAuditedEntity"/>,
        /// this method updates the modification audit metadata when the entity
        /// is in the <see cref="EntityState.Modified"/> state.
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
        }
    }
}