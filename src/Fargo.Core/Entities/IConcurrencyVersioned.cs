namespace Fargo.Core.Entities;

/// <summary>
/// Marks a mutable aggregate root as using optimistic concurrency control.
/// </summary>
/// <remarks>
/// A load-mutate-save that races with another modification fails with EF Core's
/// <c>DbUpdateConcurrencyException</c> instead of silently overwriting it (last-writer-wins). The
/// token is the PostgreSQL <c>xmin</c> column, a shadow property mapped in
/// <c>FargoDbContext.OnModelCreating</c>, so implementers expose no version member. Append-only
/// entities are never updated and do not need it.
/// </remarks>
public interface IConcurrencyVersioned
{
}
