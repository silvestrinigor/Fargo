using Fargo.Core.Activables;
using Fargo.Core.Articles;
using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;

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
public class Item : Entity, IEntityTyped, IPartitioned, IActivable
{
    public static Item CreateItem(Article article, DateTimeOffset? productionDate = null)
        => new(article, productionDate);

    /// <summary>
    /// Initializes a new item entity.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
#pragma warning disable CS9264 // Non-nullable property must contain a non-null value when exiting constructor. Consider adding the 'required' modifier, or declaring the property as nullable, or safely handling the case where 'field' is null in the 'get' accessor.
    private Item()
#pragma warning restore CS9264 // Non-nullable property must contain a non-null value when exiting constructor. Consider adding the 'required' modifier, or declaring the property as nullable, or safely handling the case where 'field' is null in the 'get' accessor.
    {
    }

    /// <summary>
    /// Initializes a new item entity associated with the specified article.
    /// </summary>
    /// <param name="article">The article associated with the item.</param>
    private Item(Article article, DateTimeOffset? productionDate = null)
    {
        if (article.IsContainer)
        {
            Container = new ItemContainer(this);
        }

        Article = article;
        ProductionDate = productionDate;
    }

    #region Article

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
    ///
    /// When this property is initialized, <see cref="ArticleGuid"/> is
    /// automatically set to the identifier of the assigned article.
    /// </remarks>
    public Article Article
    {
        get;
        init
        {
            ArticleGuid = value.Guid;
            field = value;
        }
    }

    #endregion Article

    /// <summary>
    /// Gets the date on which this item was produced.
    /// When <see langword="null"/>, the production date is unknown.
    /// </summary>
    public DateTimeOffset? ProductionDate { get; init; }

    /// <summary>
    /// Gets the date the item will expire.
    /// When <see langword="null"/>, the expiration date is unknown.
    /// </summary>
    public DateTimeOffset? ExpirationDate => ProductionDate + Article.ShelfLife;

    /// <summary>
    /// Gets a value indicating whether the item is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public EntityType GetEntityType() => EntityType.Item;

    #region Container

    /// <summary>
    /// Gets the unique identifier of the parent container item.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, the item is not currently inside another item container.
    /// </remarks>
    public Guid? ParentContainerGuid { get; private set; }

    /// <summary>
    /// Gets the parent container of the current item, if any.
    /// </summary>
    public ItemContainer? ParentContainer
    {
        get;
        internal set
        {
            if (value?.Item.Guid == Guid)
            {
                throw new ItemCannotBeOwnContainerFargoDomainException(Guid);
            }

            ParentContainerGuid = value?.Item.Guid;
            field = value;
        }
    }

    /// <summary>
    /// Gets the container information of the current item, if the current item is a container.
    /// </summary>
    public ItemContainer? Container
    {
        get;
        private init;
    }

    #endregion Container

    #region  Partition

    /// <summary>
    /// Gets the partitions associated with the item.
    /// </summary>
    /// <remarks>
    /// These partitions define the partition scope of the item and are used
    /// in partition-based access evaluation.
    /// </remarks>
    public PartitionCollection Partitions { get; init; } = [];

    public void AddPartition(Partition partition)
    {
        Partitions.Add(partition);
    }

    public void RemovePartition(Partition partition)
    {
        Partitions.Remove(partition);
    }

    /// <inheritdoc />
    IReadOnlyCollection<IPartition> IPartitioned.Partitions => Partitions;

    #endregion  Partition
}
