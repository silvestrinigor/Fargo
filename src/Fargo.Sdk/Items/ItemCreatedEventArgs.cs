namespace Fargo.Sdk.Items;

/// <summary>Provides data for the <see cref="IItemEventSource.Created"/> event.</summary>
public sealed class ItemCreatedEventArgs(Guid guid, Guid articleGuid) : EventArgs
{
    /// <summary>Gets the unique identifier of the created item.</summary>
    public Guid Guid { get; } = guid;

    /// <summary>Gets the unique identifier of the article this item is an instance of.</summary>
    public Guid ArticleGuid { get; } = articleGuid;
}
