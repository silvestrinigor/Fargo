using Fargo.Sdk.Events;

namespace Fargo.Sdk.Items;

/// <summary>Default implementation of <see cref="IItemEventSource"/>.</summary>
public sealed class ItemEventSource : IItemEventSource
{
    public ItemEventSource(IFargoEventHub hub)
    {
        hub.On<Guid, Guid>("OnItemCreated", (guid, articleGuid) =>
            Created?.Invoke(this, new ItemCreatedEventArgs(guid, articleGuid)));
    }

    public event EventHandler<ItemCreatedEventArgs>? Created;
}
