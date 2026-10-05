namespace Fargo.Core.Audits;

/// <summary>
/// Represents the association between an <see cref="AuditLog"/> and a partition
/// through which the audit log is accessible.
/// </summary>
/// <remarks>
/// Audit log partitions are derived from the partitions associated with the
/// audited entity when the audit log is created. This entity represents one
/// such association.
///
/// An audit log can be associated with multiple partitions, allowing access
/// to the audit log to follow the partition access scope of the entity that
/// was audited.
/// </remarks>
public class AuditLogPartition
{
    /// <summary>
    /// Gets the identifier of the associated audit log.
    /// </summary>
    public Guid AuditLogGuid { get; private init; }

    /// <summary>
    /// Gets the audit log associated with this partition.
    /// </summary>
    public AuditLog AuditLog { get; private init; }

    /// <summary>
    /// Gets the identifier of the partition through which the audit log
    /// is accessible.
    /// </summary>
    public Guid PartitionGuid { get; private init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private AuditLogPartition() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Initializes a new association between an audit log and a partition.
    /// </summary>
    /// <param name="auditLog">The audit log to associate with the partition.</param>
    /// <param name="partitionGuid">The identifier of the partition.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="auditLog"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="partitionGuid"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    internal AuditLogPartition(AuditLog auditLog, Guid partitionGuid)
    {
        ArgumentNullException.ThrowIfNull(auditLog);

        if (partitionGuid == Guid.Empty)
        {
            throw new ArgumentException(
                "The partition identifier cannot be empty.",
                nameof(partitionGuid));
        }

        AuditLog = auditLog;
        AuditLogGuid = auditLog.Guid;

        PartitionGuid = partitionGuid;
    }
}
