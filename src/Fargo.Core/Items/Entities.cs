using Fargo.Core.Activables;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Entities;
using Fargo.Core.Events;
using Fargo.Core.Modifiables;
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
public class Item : Entity, IModifiable, IModifiableTypes<ItemModifiedType>, IPartitioned, IActivable
{
    public static Item CreateItem(Article article, DateTimeOffset? productionDate = null)
        => new(article, productionDate);

    public static Item CreateItem(Guid guid, Article article, DateTimeOffset? productionDate = null)
        => new(article, productionDate)
        {
            Guid = guid
        };

    public static Item CreateItem(Article article, Actor actor, DateTimeOffset? productionDate = null)
    {
        ArgumentNullException.ThrowIfNull(article);
        ArgumentNullException.ThrowIfNull(actor);

        actor.ValidateHasPermission(ActionType.CreateItem);
        actor.ValidateHasAccess(article);
        article.ValidateIsActive();

        var item = new Item(article, productionDate);

        item.MarkAsEditedBy(actor.Guid);
        item.MarkModificationType(ItemModifiedType.General);

        return item;
    }

    public static Item CreateItem(Guid guid, Article article, Actor actor, DateTimeOffset? productionDate = null)
    {
        ArgumentNullException.ThrowIfNull(article);
        ArgumentNullException.ThrowIfNull(actor);

        actor.ValidateHasPermission(ActionType.CreateItem);
        actor.ValidateHasAccess(article);
        article.ValidateIsActive();

        var item = new Item(article, productionDate)
        {
            Guid = guid
        };

        item.MarkAsEditedBy(actor.Guid);
        item.MarkModificationType(ItemModifiedType.General);

        return item;
    }

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
    /// <remarks>
    /// This value is synchronized with <see cref="Article"/> when the item is initialized.
    /// </remarks>
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
        private init
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

    #region Activation

    /// <summary>
    /// Gets a value indicating whether the item is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <inheritdoc />
    public void Activate() => IsActive = true;

    public void Activate(Actor actor)
    {
        ValidateCanEdit(actor);

        if (IsActive)
        {
            return;
        }

        IsActive = true;

        MarkAsEditedBy(actor.Guid);
        MarkModificationType(ItemModifiedType.Activated);
    }

    /// <inheritdoc />
    public void Deactivate() => IsActive = false;

    public void Deactivate(Actor actor)
    {
        ValidateCanEdit(actor);

        if (!IsActive)
        {
            return;
        }

        IsActive = false;

        MarkAsEditedBy(actor.Guid);
        MarkModificationType(ItemModifiedType.Deactivated);
    }

    #endregion Activation

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

    public void AddPartition(Partition partition, Actor actor)
    {
        ArgumentNullException.ThrowIfNull(partition);

        ValidateCanEdit(actor);
        actor.ValidateHasAccess(partition.Guid);

        if (Partitions.Any(p => p.Guid == partition.Guid))
        {
            return;
        }

        Partitions.Add(partition);

        MarkAsEditedBy(actor.Guid);
        MarkModificationType(ItemModifiedType.PartitionsChanged);
    }

    public void RemovePartition(Partition partition)
    {
        Partitions.Remove(partition);
    }

    public void RemovePartition(Partition partition, Actor actor)
    {
        ArgumentNullException.ThrowIfNull(partition);

        ValidateCanEdit(actor);
        actor.ValidateHasAccess(partition.Guid);

        if (!Partitions.Any(p => p.Guid == partition.Guid))
        {
            return;
        }

        Partitions.Remove(partition);

        MarkAsEditedBy(actor.Guid);
        MarkModificationType(ItemModifiedType.PartitionsChanged);
    }

    /// <inheritdoc />
    IReadOnlyCollection<IPartition> IPartitioned.Partitions => Partitions;

    #endregion  Partition

    #region Modified

    public void ValidateCanEdit(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);

        actor.ValidateHasPermission(ActionType.EditItem);
        actor.ValidateHasAccess(this);
    }

    public void ValidateCanDelete(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);

        actor.ValidateHasPermission(ActionType.DeleteItem);
        actor.ValidateHasAccess(this);
    }

    public Guid? EditedByActorid { get; private set; }

    public void MarkAsEditedBy(Guid actorGuid)
    {
        EditedByActorid = actorGuid;
    }

    public ItemModifiedType ModificationTypes { get; private set; }

    public IReadOnlySet<ItemModifiedType> GetModificationTypes()
    {
        HashSet<ItemModifiedType> result = [];

        foreach (ItemModifiedType value in Enum.GetValues<ItemModifiedType>())
        {
            if (value == ItemModifiedType.None)
            {
                continue;
            }

            if ((ModificationTypes & value) != 0)
            {
                result.Add(value);
            }
        }

        return result;
    }

    public bool IsEditStarted { get; private set; }

    public void MarkModificationType(ItemModifiedType modificationType)
    {
        if (!IsEditStarted)
        {
            ModificationTypes = ItemModifiedType.None;
            IsEditStarted = true;
        }

        ModificationTypes |= modificationType;
    }

    #endregion Modified
}

/// <summary>
/// Represents the container behavior of an <see cref="Item"/>.
/// </summary>
/// <remarks>
/// An item container exists only when the associated item's article is defined
/// as a container article.
/// </remarks>
public sealed class ItemContainer
{
    /// <summary>
    /// Initializes a new item container for the specified item.
    /// </summary>
    /// <param name="item">The item that owns this container information.</param>
    public ItemContainer(Item item)
    {
        Item = item;
    }

    /// <summary>
    /// Gets the item that owns this container information.
    /// </summary>
    public Item Item { get; private init; }
}

public sealed class ItemBatch : Entity
{
    /// <summary>
    /// Gets the date on which this batch was produced.
    /// When <see langword="null"/>, the production date is unknown.
    /// </summary>
    public DateTimeOffset? ProductionDate { get; init; }
}

/// <summary>
/// Represents item movement details attached to an entity event.
/// </summary>
/// <remarks>
/// The related <see cref="Events.Event"/> stores the moved item, actor, and occurrence time.
/// This row stores only the movement-specific location details.
/// </remarks>
public sealed class ItemMovement : Entity
{
    /// <summary>
    /// Initializes a new item movement entity.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
    private ItemMovement()
    {
    }

    private ItemMovement(
        Event entityEvent,
        Guid? fromParentContainerGuid,
        Guid? toParentContainerGuid)
    {
        Event = entityEvent;
        Guid = entityEvent.Guid;
        FromParentContainerGuid = fromParentContainerGuid;
        ToParentContainerGuid = toParentContainerGuid;
    }

    /// <summary>
    /// Creates a new item movement detail and its related movement event.
    /// </summary>
    public static ItemMovement Moved(
        Item item,
        Guid? fromParentContainerGuid,
        Guid? toParentContainerGuid,
        Guid actorGuid,
        DateTimeOffset? occurredAt = null)
        => new(
            Event.ItemMoved(item, actorGuid, occurredAt),
            fromParentContainerGuid,
            toParentContainerGuid);

    /// <summary>
    /// Gets the related entity event.
    /// </summary>
    public Event Event { get; private init; } = null!;

    /// <summary>
    /// Gets the moved item unique identifier.
    /// </summary>
    public Guid ItemGuid => Event.EntityGuid;

    /// <summary>
    /// Gets the previous parent container item unique identifier.
    /// </summary>
    public Guid? FromParentContainerGuid { get; private init; }

    /// <summary>
    /// Gets the new parent container item unique identifier.
    /// </summary>
    public Guid? ToParentContainerGuid { get; private init; }

    /// <summary>
    /// Gets the actor unique identifier that performed the movement.
    /// </summary>
    public Guid ActorGuid => Event.ActorGuid;

    /// <summary>
    /// Gets the date and time when the movement occurred.
    /// </summary>
    public DateTimeOffset OccurredAt => Event.OccurredAt;
}
