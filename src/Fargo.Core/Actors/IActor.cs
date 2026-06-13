using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.Shared.Actors;

namespace Fargo.Core.Actors;

public interface IActor
{
    ActorId ActorId { get; }

    bool HasPermission(ActionType action);

    bool HasPartitionAccess(Guid partitionGuid);

    bool HasAccess(IPartition partition);

    bool HasAccess(IPartitioned partitioned);

    bool HasAccess(IPartitionedGuids partitioned);
}
