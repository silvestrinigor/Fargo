namespace Fargo.Core.Entities;

/// <summary>
/// Serves as the abstract base for every first-class entity in the system.
/// </summary>
/// <remarks>
/// Each concrete aggregate (such as <c>Article</c>, <c>Item</c>, <c>Actor</c>,
/// <c>UserGroup</c> and <c>Partition</c>) derives from this type so that every
/// entity is represented by a single row in the shared <c>entities</c> table,
/// keyed by its <see cref="Guid"/> and tagged with its <see cref="EntityType"/>.
///
/// The persistence layer maps this hierarchy with the Entity Framework Core
/// Table-Per-Type (TPT) strategy: the base row lives in <c>entities</c> and the
/// kind-specific columns live in the derived table, whose primary key is also a
/// foreign key back to <c>entities.guid</c>. This gives a one-to-one
/// relationship between an entity and its concrete representation (for example
/// one article is backed by exactly one entity of type
/// <see cref="EntityType.Article"/>).
///
/// Not every persisted record is an <see cref="Entity"/>. Append-only records
/// such as audit logs and item events are intentionally kept outside this
/// hierarchy and therefore have no row in <c>entities</c>.
/// </remarks>
public abstract class Entity
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    /// <remarks>
    /// Defaults to a freshly generated value. Derived types may assign a
    /// well-known identifier (for example the global partition or the built-in
    /// administrator) through an object initializer or constructor.
    /// </remarks>
    public Guid Guid { get; protected init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the type of the entity.
    /// </summary>
    /// <remarks>
    /// Set once by the derived type and persisted on the entity's row in the
    /// shared <c>entities</c> table.
    /// </remarks>
    public EntityType EntityType { get; protected init; }
}
