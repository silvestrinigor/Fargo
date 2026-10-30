namespace Fargo.Core.Entities;

/// <summary>
/// Represents a entity type.
/// </summary>
public enum EntityType
{
    /// <summary>
    /// Article entity.
    /// </summary>
    Article = 0,

    /// <summary>
    /// Item entity.
    /// </summary>
    Item = 1,

    /// <summary>
    /// User entity.
    /// </summary>
    User = 2,

    /// <summary>
    /// User group entity.
    /// </summary>
    UserGroup = 3,

    /// <summary>
    /// Partition entity.
    /// </summary>
    Partition = 4,

    /// <summary>
    /// Audit log entity.
    /// </summary>
    AuditLog = 5,

    /// <summary>
    /// Item event entity.
    /// </summary>
    ItemEvent = 6,

    /// <summary>
    /// Service account entity (a non-interactive machine identity).
    /// </summary>
    ServiceAccount = 7
}
