namespace Fargo.Sdk.Items;

/// <summary>Broadcasts the hub <c>OnItemCreated</c> event as a typed .NET event.</summary>
public interface IItemEventSource
{
    /// <summary>Raised when any authenticated client creates an item.</summary>
    event EventHandler<ItemCreatedEventArgs>? Created;
}
