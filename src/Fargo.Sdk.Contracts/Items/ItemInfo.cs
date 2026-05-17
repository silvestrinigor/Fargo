namespace Fargo.Sdk.Contracts.Items;

/// <summary>Represents the item areas changed by the last edit operation.</summary>
[Flags]
public enum ItemModifiedType
{
    None = 0,
    General = 1 << 0,
    ParentContainerChanged = 1 << 1,
    PartitionsChanged = 1 << 2,
}

/// <summary>Represents an item returned by the API.</summary>
public sealed record ItemInfo(
    Guid Guid,
    Guid ArticleGuid,
    DateTimeOffset? ProductionDate = null,
    Guid? ParentContainerGuid = null,
    IReadOnlyCollection<Guid> Partitions = null!,
    Guid? EditedByGuid = null,
    ItemModifiedType ModificationTypes = ItemModifiedType.None);
