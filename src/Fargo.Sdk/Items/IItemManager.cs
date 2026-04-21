namespace Fargo.Sdk.Items;

/// <summary>
/// Provides high-level operations for managing items.
/// Returns live <see cref="Item"/> entities that expose partition access as a method.
/// </summary>
public interface IItemManager
{
    /// <summary>Gets a single item by its unique identifier.</summary>
    /// <exception cref="FargoSdkApiException">Thrown if the item does not exist or is not accessible.</exception>
    Task<Item> GetAsync(
        Guid itemGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a paginated list of items accessible to the current user.</summary>
    /// <param name="articleGuid">When provided, returns only items associated with this article.</param>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task<IReadOnlyCollection<Item>> GetManyAsync(
        Guid? articleGuid = null,
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        Guid? partitionGuid = null,
        bool? noPartition = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a new item as an instance of the specified article.</summary>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task<Item> CreateAsync(
        Guid articleGuid,
        Guid? firstPartition = null,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes an item.</summary>
    /// <exception cref="FargoSdkApiException">Thrown if the item cannot be deleted or is not accessible.</exception>
    Task DeleteAsync(
        Guid itemGuid,
        CancellationToken cancellationToken = default);

    /// <summary>Raised when any authenticated client creates an item.</summary>
    event EventHandler<ItemCreatedEventArgs>? Created;
}
