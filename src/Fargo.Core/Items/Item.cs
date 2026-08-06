using Fargo.Core.Articles;
using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Fargo.Core.Shared.Articles;

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
/// </remarks>
public class Item : IEntity, IPartitionedReadOnly
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
    /// When <see langword="null"/>, the item is not currently inside another item container.
    /// </remarks>
    public Guid? ParentItemContainerGuid { get; private set; }

    /// <summary>
    /// Gets the container item that directly contains this item, if any.
    /// </summary>
    /// <remarks>
    /// A <see langword="null"/> value indicates that the item is not currently
    /// contained within another item.
    /// </remarks>
    public Item? ParentItemContainer { get; private set; }

    /// <summary>
    /// Gets the partitions directly associated with the item.
    /// </summary>
    /// <remarks>
    /// These partitions define the partition scope of the item and are used
    /// in partition-based access evaluation.
    /// </remarks>
    public IReadOnlyCollection<Partition> Partitions => partitions;
    private readonly List<Partition> partitions = [];

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
    private Item(Article article)
    {
        Article = article;
        ArticleGuid = article.Guid;
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
    /// <param name="itemContainer">
    /// The parent container item.
    /// </param>
    /// <remarks>
    /// Repository-dependent validation, such as preventing circular container
    /// hierarchies and verifying that the parent is a container item, must be
    /// performed before calling this method.
    /// </remarks>
    public void SetParentItemContainer(Item itemContainer)
    {
        if (itemContainer.Guid == Guid)
        {
            throw new FargoCoreException($"Item '{Guid}' cannot be its own parent container.");
        }

        if (itemContainer.Article.ArticleType != ArticleType.Container)
        {
            throw new FargoCoreException(
                $"Item '{itemContainer.Guid}' is not a container item.",
                FargoCoreErrorType.InvalidArgument);
        }

        ParentItemContainer = itemContainer;
        ParentItemContainerGuid = itemContainer.Guid;
    }

    /// <summary>
    /// Removes the item from its parent container.
    /// </summary>
    /// <remarks>
    /// After calling this method, the item is no longer contained within another
    /// item.
    /// </remarks>
    public void RemoveItemFromParentItemContainer()
    {
        ParentItemContainer = null;
        ParentItemContainerGuid = null;
    }

    /// <summary>
    /// Associates the item with the specified partition.
    /// </summary>
    /// <param name="partition">The partition to associate.</param>
    public void AddPartition(Partition partition)
    {
        if (partitions.Any(p => p.Guid == partition.Guid))
        {
            return;
        }

        partitions.Add(partition);
    }

    /// <summary>
    /// Removes the association between the item and the specified partition.
    /// </summary>
    /// <param name="partitionGuid">
    /// The identifier of the partition to remove.
    /// </param>
    public void RemovePartition(Guid partitionGuid)
    {
        partitions.RemoveAll(p => p.Guid == partitionGuid);
    }
}
