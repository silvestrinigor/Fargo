namespace Fargo.Sdk.Items;

/// <summary>Provides data for the <see cref="IItemManager.Deleted"/> event.</summary>
public sealed class ItemDeletedEventArgs(Guid guid) : EventArgs
{
    /// <summary>Gets the unique identifier of the deleted item.</summary>
    public Guid Guid { get; } = guid;
}
