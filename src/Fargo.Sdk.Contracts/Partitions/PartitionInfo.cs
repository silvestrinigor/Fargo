namespace Fargo.Sdk.Contracts.Partitions;

/// <summary>Represents the partition areas changed by the last edit operation.</summary>
[Flags]
public enum PartitionModifiedType
{
    None = 0,
    General = 1 << 0,
    ParentChanged = 1 << 1,
    Activated = 1 << 2,
    Deactivated = 1 << 3,
}

/// <summary>Represents a partition returned by the API.</summary>
public sealed record PartitionInfo(
    Guid Guid,
    string Name,
    string Description,
    Guid? ParentPartitionGuid,
    bool IsActive,
    Guid? EditedByGuid = null,
    PartitionModifiedType ModificationTypes = PartitionModifiedType.None);
