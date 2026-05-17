namespace Fargo.Core.Items;

[Flags]
public enum ItemModifiedType
{
    None = 0,
    General = 1 << 0,
    ParentContainerChanged = 1 << 1,
    PartitionsChanged = 1 << 2,
}
