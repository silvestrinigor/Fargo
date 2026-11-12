namespace Fargo.Core.Entities;

/// <summary>
/// Marks an aggregate root as participating in optimistic concurrency control.
/// </summary>
/// <remarks>
/// Entities implementing this interface are configured with a concurrency token
/// (the PostgreSQL <c>xmin</c> system column) so that a load-mutate-save that
/// races with a concurrent modification fails with a concurrency exception
/// (EF Core's <c>DbUpdateConcurrencyException</c>) instead of silently
/// overwriting the other change (last-writer-wins).
///
/// This marker is applied only to mutable aggregate roots. Append-only entities
/// (audit logs, item events, refresh tokens, authentication attempts) are
/// inserted rather than updated, so a concurrency token adds no value to them.
///
/// The token itself is a shadow property mapped in
/// <c>FargoDbContext.OnModelCreating</c>; implementers do not expose a version
/// member on the domain model.
/// </remarks>
public interface IConcurrencyVersioned
{
}
