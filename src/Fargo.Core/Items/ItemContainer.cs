namespace Fargo.Core.Items;

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

    public Guid ItemGuid { get; private init; }

    /// <summary>
    /// Gets the item that owns this container information.
    /// </summary>
    public Item Item
    {
        get;
        private init
        {
            ItemGuid = value.Guid;

            field = value;
        }
    }
}
