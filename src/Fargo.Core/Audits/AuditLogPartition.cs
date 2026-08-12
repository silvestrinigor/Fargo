namespace Fargo.Core.Audits;

public class AuditLogPartition
{
    public Guid AuditLogGuid { get; private init; }

    public AuditLog AuditLog { get; private init; }

    public Guid PartitionGuid { get; private init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private AuditLogPartition() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    internal AuditLogPartition(AuditLog auditLog, Guid partitionGuid)
    {
        AuditLog = auditLog;
        AuditLogGuid = auditLog.Guid;

        PartitionGuid = partitionGuid;
    }
}
