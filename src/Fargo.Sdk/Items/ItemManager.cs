namespace Fargo.Sdk.Items;

/// <summary>Delegate façade that implements <see cref="IItemManager"/> by composing the focused services.</summary>
public sealed class ItemManager(IItemService service, IItemEventSource eventSource) : IItemManager
{
    public event EventHandler<ItemCreatedEventArgs>? Created
    {
        add => eventSource.Created += value;
        remove => eventSource.Created -= value;
    }

    public Task<Item> GetAsync(Guid itemGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
        => service.GetAsync(itemGuid, temporalAsOf, cancellationToken);

    public Task<IReadOnlyCollection<Item>> GetManyAsync(Guid? articleGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, Guid? partitionGuid = null, bool? noPartition = null, CancellationToken cancellationToken = default)
        => service.GetManyAsync(articleGuid, temporalAsOf, page, limit, partitionGuid, noPartition, cancellationToken);

    public Task<Item> CreateAsync(Guid articleGuid, Guid? firstPartition = null, CancellationToken cancellationToken = default)
        => service.CreateAsync(articleGuid, firstPartition, cancellationToken);

    public Task DeleteAsync(Guid itemGuid, CancellationToken cancellationToken = default)
        => service.DeleteAsync(itemGuid, cancellationToken);
}
