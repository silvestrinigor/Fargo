using Fargo.Domain.Enums;

namespace Fargo.Domain.Security;

public abstract class Actor : IActor
{
    abstract public Guid Guid { get; }

    abstract public bool IsAdmin { get; }

    abstract public bool IsSystem { get; }

    abstract public IReadOnlyCollection<ActionType> PermissionActions { get; }

    abstract public IReadOnlyCollection<Guid> PartitionAccesses { get; }

    public bool HasActionPermission(ActionType action)
    {
        return PermissionActions.Any(p => p == action);
    }

    public bool HasPartitionAccess(Guid partitionGuid)
    {
        return PartitionAccesses.Any(p => p == partitionGuid);
    }
}
