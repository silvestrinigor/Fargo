namespace Fargo.Domain.Entities;

/// <summary>
/// Defines a contract for entities that are members of an audited aggregate
/// and whose changes should update the audit metadata of the aggregate root.
/// </summary>
/// <remarks>
/// Implementations of this interface indicate that modifications to the
/// current entity should propagate auditing updates to a parent entity
/// that implements <see cref="IAuditedEntity"/>.
///
/// This is typically used for aggregate members whose lifecycle and
/// persistence are controlled by an audited aggregate root.
/// </remarks>
public interface IAuditedAggregateMember
{
    /// <summary>
    /// Gets the parent audited entity whose audit metadata must be updated
    /// when this entity changes.
    /// </summary>
    IAuditedEntity ParentAuditedEntity { get; }
}
