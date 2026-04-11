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
        {
            if (_tracked.TryGetValue(guid, out var item))
                item.RaiseUpdated();
        });

        hub.On<Guid>("OnItemDeleted", guid =>
        {
            if (_tracked.TryGetValue(guid, out var item))
                item.RaiseDeleted();
        });
    }

    public event EventHandler<ItemCreatedEventArgs>? Created;

    private readonly Dictionary<Guid, Item> _tracked = new();
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

        var item = new Item(response.Data, articleGuid, client);
        _tracked[item.Guid] = item;
        return item;
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

    private Item ToEntity(ItemResult r)
    {
        var item = new Item(r.Guid, r.ArticleGuid, client);
        _tracked[item.Guid] = item;
        return item;
    }

    private static void ThrowError(FargoSdkError error) =>
        throw new FargoSdkApiException(error.Detail);
}
