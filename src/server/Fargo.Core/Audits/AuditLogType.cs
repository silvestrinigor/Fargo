namespace Fargo.Core.Audits;

/// <summary>
/// Represents the kind of change an audit log records.
/// </summary>
/// <remarks>
/// Audit log types describe the lifecycle event that occurred on the audited
/// entity. The specific entity affected is captured separately by the audit
/// log's entity type, so these values stay generic (created, updated, deleted).
/// </remarks>
public enum AuditLogType
{
    /// <summary>
    /// The entity was created.
    /// </summary>
    EntityCreated = 1,

    /// <summary>
    /// The entity was updated.
    /// </summary>
    EntityUpdated = 2,

    /// <summary>
    /// The entity was deleted.
    /// </summary>
    EntityDeleted = 3,

    /// <summary>
    /// An event was recorded against an event-tracked entity, such as a
    /// movement of a quantity of an event-tracked article.
    /// </summary>
    ItemEventRecorded = 4,
}
