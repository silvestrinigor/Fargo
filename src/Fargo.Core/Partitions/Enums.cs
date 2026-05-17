namespace Fargo.Core.Partitions;

[Flags]
public enum PartitionModifiedType
{
    None = 0,
    General = 1 << 0,
    ParentChanged = 1 << 1,
    Activated = 1 << 2,
    Deactivated = 1 << 3,
}
