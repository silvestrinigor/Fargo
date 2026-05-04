namespace Fargo.Api.Items;

public interface IItemManager
{
    Task<Item> GetAsync(Guid itemGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Item>> GetManyAsync(
        Guid? articleGuid = null,
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        Guid? partitionGuid = null,
        bool? noPartition = null,
        CancellationToken cancellationToken = default);

    Task<Item> CreateAsync(Guid articleGuid, Guid? firstPartition = null, DateTimeOffset? productionDate = null, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid itemGuid, CancellationToken cancellationToken = default);
}

public sealed class Item
{
    private readonly IItemHttpClient client;

    internal Item(ItemResult result, IItemHttpClient client)
    {
        this.client = client;
        Guid = result.Guid;
        ArticleGuid = result.ArticleGuid;
        ProductionDate = result.ProductionDate;
        EditedByGuid = result.EditedByGuid;
    }

    public Guid Guid { get; }

    public Guid ArticleGuid { get; }

    public DateTimeOffset? ProductionDate { get; set; }

    public Guid? EditedByGuid { get; }

    public async Task UpdateAsync(Action<Item> update, CancellationToken cancellationToken = default)
    {
        update(this);
        (await client.UpdateAsync(Guid, ProductionDate, cancellationToken)).EnsureSuccess("Failed to update item.");
    }

    public Task<FargoSdkResponse<EmptyResult>> AddPartitionAsync(Guid partitionGuid, CancellationToken cancellationToken = default)
        => client.AddPartitionAsync(Guid, partitionGuid, cancellationToken);

    public Task<FargoSdkResponse<EmptyResult>> RemovePartitionAsync(Guid partitionGuid, CancellationToken cancellationToken = default)
        => client.RemovePartitionAsync(Guid, partitionGuid, cancellationToken);
}

public sealed class ItemManager(IItemHttpClient client) : IItemManager
{
    public async Task<Item> GetAsync(Guid itemGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
        => new((await client.GetAsync(itemGuid, temporalAsOf, cancellationToken)).Unwrap("Failed to load item."), client);

    public async Task<IReadOnlyCollection<Item>> GetManyAsync(
        Guid? articleGuid = null,
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        Guid? partitionGuid = null,
        bool? noPartition = null,
        CancellationToken cancellationToken = default)
        => (await client.GetManyAsync(articleGuid, temporalAsOf, page, limit, partitionGuid, noPartition, cancellationToken))
            .Unwrap("Failed to load items.")
            .Select(x => new Item(x, client))
            .ToArray();

    public async Task<Item> CreateAsync(Guid articleGuid, Guid? firstPartition = null, DateTimeOffset? productionDate = null, CancellationToken cancellationToken = default)
    {
        var guid = (await client.CreateAsync(articleGuid, firstPartition, productionDate, cancellationToken)).Unwrap("Failed to create item.");
        return await GetAsync(guid, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(Guid itemGuid, CancellationToken cancellationToken = default)
        => (await client.DeleteAsync(itemGuid, cancellationToken)).EnsureSuccess("Failed to delete item.");
}
