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
    public Guid Guid { get; private init; } = Guid.NewGuid();

    public ItemContainer()
    {
    }
}
