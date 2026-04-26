namespace Fargo.Sdk.Items;

/// <summary>Provides data for the <see cref="Item.Updated"/> event.</summary>
public sealed class ItemUpdatedEventArgs(Guid guid) : EventArgs
{
    /// <summary>Gets the unique identifier of the updated item.</summary>
    public Guid Guid { get; } = guid;
}
