using Fargo.Core.Articles;
using Fargo.Core.Common;
using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Fargo.Core.Shared.Articles;
using Fargo.Core.Shared.Common;
using Fargo.Core.Shared.Entities;

namespace Fargo.Core.Items;

/// <summary>
/// Represents an item in the system.
/// </summary>
/// <remarks>
/// An item is a concrete instance associated with a specific
/// <see cref="Article"/>. While an <see cref="Article"/> defines the
/// descriptive information of a product type, an <see cref="Item"/>
/// represents an individual unit of that article.
///
/// An item is partitioned data and defines its own partition scope
/// independently of the associated <see cref="Article"/>.
///
/// Although the item is related to an article, access to the item is not
/// determined by the article's partitions. Instead, a user may access the item
/// if the item has no partition (public), or if they have access to at least
/// one partition associated directly with the item.
///
/// Every item is always associated with the global partition. The global
/// partition defines the base partition scope of the item and cannot be removed.
/// </remarks>
public class Item : IEntity, IEntityTyped, IPartitionedGuidsReadOnly
{
    /// <summary>
    /// Gets the unique identifier of the item.
    /// </summary>
    public Guid Guid { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the unique identifier of the associated <see cref="Article"/>.
    /// </summary>
    public Guid ArticleGuid { get; private init; }

    /// <summary>
    /// Gets the article associated with this item.
    /// </summary>
    /// <remarks>
    /// The associated article defines the descriptive classification of the item,
    /// but does not determine the partition access scope of this entity.
    /// </remarks>
    public Article Article { get; private init; }

    /// <summary>
    /// Gets the unique identifier of the parent container item.
    /// </summary>
    /// <remarks>
    /// A <see langword="null"/> value indicates that the item is not currently
    /// placed inside another item.
    /// </remarks>
    public Guid? ParentItemContainerGuid { get; private set; }

    /// <summary>
    /// Gets the container item that directly contains this item, if any.
    /// </summary>
    /// <remarks>
    /// Items can be placed inside container items, allowing nested containment
    /// relationships where a container may itself be placed inside another
    /// container.
    ///
    /// A <see langword="null"/> value indicates that the item is not currently
    /// placed inside another item.
    /// </remarks>
    public Item? ParentItemContainer { get; private set; }

    public bool IsFixed { get; private set; } = false;

    /// <summary>
    /// Gets the partitions directly associated with the item.
    /// </summary>
    /// <remarks>
    /// Every item is always associated with the global partition.
    /// The global partition defines the base partition scope of the item
    /// and cannot be removed.
    ///
    /// Additional partitions may be associated with the item.
    /// </remarks>
    public IReadOnlyCollection<ItemPartition> Partitions => partitions;

    /// <summary>
    /// Gets the unique identifiers of the partitions associated with the item.
    /// </summary>
    public IReadOnlyCollection<Guid> PartitionGuids => [.. partitions.Select(p => p.PartitionGuid)];

    private readonly List<ItemPartition> partitions = [];

    /// <summary>
    /// Initializes a new item entity.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Item()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
    }

    /// <summary>
    /// Initializes a new item entity associated with the specified article.
    /// </summary>
    /// <param name="article">The article associated with the item.</param>
    /// <remarks>
    /// Every item is automatically associated with the global partition when
    /// it is created. This association is mandatory and cannot be removed.
    /// </remarks>
    private Item(Article article)
    {
        Article = article;
        ArticleGuid = article.Guid;

        partitions.Add(new ItemPartition(this, FargoCoreWellKnowGuids.GlobalPartitionGuid));
    }

    /// <summary>
    /// Creates a new item associated with the specified <paramref name="article"/>.
    /// </summary>
    /// <param name="article">The article associated with the item.</param>
    /// <returns>A new <see cref="Item"/> instance.</returns>
    public static Item CreateItem(Article article)
        => new(article);

    /// <summary>
    /// Assigns the specified container item as the parent of the current item.
    /// </summary>
    /// <param name="parentItemContainer">
    /// The parent container item.
    /// </param>
    /// <remarks>
    /// This method does not validate the complete item containment hierarchy.
    /// The application should use <see cref="ItemService"/> to validate that
    /// assigning the parent container does not create a circular hierarchy.
    /// </remarks>
    public void PlaceInsideContainer(Item parentItemContainer)
    {
        ArgumentNullException.ThrowIfNull(parentItemContainer);

        if (parentItemContainer.Guid == Guid)
        {
            throw new FargoCoreException($"Item '{Guid}' cannot be its own parent container.", FargoErrorType.InvalidOperation);
        }

        if (parentItemContainer.Article.ArticleType != ArticleType.Container)
        {
            throw new FargoCoreException(
                $"Item '{parentItemContainer.Guid}' is not a container item.", FargoErrorType.InvalidOperation);
        }

        if (IsFixed)
        {
            throw new FargoCoreException(
                $"The fixed item {Guid} cannot be moved to container {parentItemContainer.Guid}.",
                FargoErrorType.InvalidOperation);
        }

        ParentItemContainer = parentItemContainer;

        ParentItemContainerGuid = parentItemContainer.Guid;
    }

    /// <summary>
    /// Removes the parent container association from this item.
    /// </summary>
    /// <remarks>
    /// After calling this method, the item is no longer placed inside another
    /// container item.
    /// </remarks>
    public void RemoveParentItemContainer()
    {
        if (IsFixed)
        {
            throw new FargoCoreException(
                $"The fixed item {Guid} cannot be removed from container.",
                FargoErrorType.InvalidOperation);
        }

        ParentItemContainer = null;

        ParentItemContainerGuid = null;
    }

    public void Fix()
    {
        IsFixed = true;
    }

    public void Unfix()
    {
        IsFixed = false;
    }


    /// <summary>
    /// Associates the item with the specified partition.
    /// If the association already exists, no action is taken.
    /// </summary>
    /// <param name="partition">The partition to associate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="partition"/> is <see langword="null"/>.
    /// </exception>
    public void AddPartition(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (partitions.Any(p => p.PartitionGuid == partition.Guid))
        {
            return;
        }

        partitions.Add(new ItemPartition(this, partition));
    }


    /// <summary>
    /// Removes the association between the item and the specified partition.
    /// </summary>
    /// <remarks>
    /// The global partition is mandatory for every item and therefore cannot
    /// be removed.
    ///
    /// If the item is not associated with the specified partition,
    /// no action is taken.
    /// </remarks>
    /// <param name="partitionGuid">
    /// The identifier of the partition to remove.
    /// </param>
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to remove the global partition.
    /// </exception>
    public void RemovePartition(Guid partitionGuid)
    {
        if (partitionGuid == FargoCoreWellKnowGuids.GlobalPartitionGuid)
        {
            throw new FargoCoreException(
                "The global partition is mandatory and cannot be removed from an item.",
                FargoErrorType.InvalidOperation);
        }

        partitions.RemoveAll(p => p.PartitionGuid == partitionGuid);
    }

    public EntityType GetEntityType() => EntityType.Item;
}
