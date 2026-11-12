namespace Fargo.Core.Entities;

/// <summary>
/// Base of every first-class entity.
/// </summary>
/// <remarks>
/// Every entity has one row in the shared <c>entities</c> table, keyed by <see cref="Guid"/> and
/// tagged with its <see cref="EntityType"/>. Derived tables (EF Core Table-Per-Type) reference it
/// through a foreign key on <c>entities.guid</c>. Append-only records such as audit logs and item
/// events are deliberately not entities and have no row there.
/// </remarks>
public abstract class Entity
{
    /// <summary>
    /// Derived types may assign a well-known identifier (e.g. the global partition or the built-in
    /// administrator) instead of the generated one.
    /// </summary>
    public Guid Guid { get; protected init; } = Guid.NewGuid();

    public EntityType EntityType { get; protected init; }
}
