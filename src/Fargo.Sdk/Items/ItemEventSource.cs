using Fargo.Sdk.Events;

namespace Fargo.Sdk.Items;

/// <summary>Default implementation of <see cref="IItemEventSource"/>.</summary>
public sealed class ItemEventSource : IItemEventSource
{
    /// <summary>Initializes a new instance.</summary>
    public ItemEventSource(IFargoEventHub hub)
    {
        hub.On<Guid, Guid>("OnItemCreated", (guid, articleGuid) =>
            Created?.Invoke(this, new ItemCreatedEventArgs(guid, articleGuid)));
    }

    /// <inheritdoc />
    public event EventHandler<ItemCreatedEventArgs>? Created;
}
