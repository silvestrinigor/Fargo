using Fargo.Sdk.Events;

namespace Fargo.Sdk.Items;

/// <summary>Default implementation of <see cref="IItemManager"/>.</summary>
public sealed class ItemManager : IItemManager
{
    internal ItemManager(IItemClient client, FargoHubConnection hub)
    {
        this.client = client;
        this.hub = hub;

        hub.On<Guid, Guid>("OnItemCreated", (guid, articleGuid) =>
            Created?.Invoke(this, new ItemCreatedEventArgs(guid, articleGuid)));

        hub.On<Guid>("OnItemUpdated", guid =>
        {
            if (_tracked.TryGetValue(guid, out var item))
            {
                item.RaiseUpdated();
            }
        });

        hub.On<Guid>("OnItemDeleted", guid =>
        {
            if (_tracked.TryGetValue(guid, out var item))
            {
                item.RaiseDeleted();
            }
        });
    }

    public event EventHandler<ItemCreatedEventArgs>? Created;

    private readonly Dictionary<Guid, Item> _tracked = new();
    private readonly IItemClient client;
    private readonly FargoHubConnection hub;

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

        return await ToEntityAsync(response.Data!);
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

        var entities = new List<Item>();
        foreach (var r in response.Data!)
        {
            entities.Add(await ToEntityAsync(r));
        }

        return entities;
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

        var item = new Item(response.Data, articleGuid, client, MakeDisposeCallback(response.Data));
        _tracked[item.Guid] = item;
        await hub.InvokeAsync("SubscribeToEntityAsync", item.Guid);
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

    private async Task<Item> ToEntityAsync(ItemResult r)
    {
        var item = new Item(r.Guid, r.ArticleGuid, client, MakeDisposeCallback(r.Guid));
        _tracked[item.Guid] = item;
        await hub.InvokeAsync("SubscribeToEntityAsync", item.Guid);
        return item;
    }

    private Func<ValueTask> MakeDisposeCallback(Guid guid) => async () =>
    {
        _tracked.Remove(guid);
        await hub.InvokeAsync("UnsubscribeFromEntityAsync", guid);
    };

    private static void ThrowError(FargoSdkError error) =>
        throw new FargoSdkApiException(error.Detail);
}
