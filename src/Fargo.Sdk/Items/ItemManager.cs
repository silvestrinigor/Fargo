using Fargo.Sdk.Events;

namespace Fargo.Sdk.Items;

/// <summary>Default implementation of <see cref="IItemManager"/>.</summary>
public sealed class ItemManager : IItemManager
{
    internal ItemManager(IItemClient client, FargoHubConnection hub)
    {
        this.client = client;

        hub.On<Guid, Guid>("OnItemCreated", (guid, articleGuid) =>
            Created?.Invoke(this, new ItemCreatedEventArgs(guid, articleGuid)));

        hub.On<Guid>("OnItemUpdated", guid =>
            Updated?.Invoke(this, new ItemUpdatedEventArgs(guid)));

        hub.On<Guid>("OnItemDeleted", guid =>
            Deleted?.Invoke(this, new ItemDeletedEventArgs(guid)));
    }

    public event EventHandler<ItemCreatedEventArgs>? Created;
    public event EventHandler<ItemUpdatedEventArgs>? Updated;
    public event EventHandler<ItemDeletedEventArgs>? Deleted;

    private readonly IItemClient client;

    public async Task<Item> GetAsync(
        Guid itemGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync(itemGuid, temporalAsOf, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        return ToEntity(response.Data!);
    }

    public async Task<IReadOnlyCollection<Item>> GetManyAsync(
        Guid? articleGuid = null,
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetManyAsync(articleGuid, temporalAsOf, page, limit, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        return response.Data!.Select(ToEntity).ToList();
    }

    public async Task<Item> CreateAsync(
        Guid articleGuid,
        Guid? firstPartition = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.CreateAsync(articleGuid, firstPartition, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        return new Item(response.Data, articleGuid, client);
    }

    public async Task DeleteAsync(
        Guid itemGuid,
        CancellationToken cancellationToken = default)
    {
        var response = await client.DeleteAsync(itemGuid, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }
    }

    private Item ToEntity(ItemResult r) => new(r.Guid, r.ArticleGuid, client);

    private static void ThrowError(FargoSdkError error) =>
        throw new FargoSdkApiException(error.Detail);
}
