using Fargo.Domain.Logics;

namespace Fargo.Domain.Entities;

public class UserGroupPartitionAccess : Entity, IModifiedEntityMember, IPartitionAccess
{
    public Guid UserGroupGuid
    {
        get;
        private set;
    }

    public required UserGroup UserGroup
    {
        get;
        set
        {
            UserGroupGuid = value.Guid;
            field = value;
        }
    }

    public Guid PartitionGuid
    {
        get;
        private set;
    }

    public required Partition Partition
    {
        get;
        set
        {
            PartitionGuid = value.Guid;
            field = value;
        }
    }

    IPartition IPartitionAccess.Partition => Partition;

    public IModifiedEntity ParentEditedEntity => UserGroup;
}
