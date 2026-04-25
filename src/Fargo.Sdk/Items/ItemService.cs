using Fargo.Sdk.Events;

namespace Fargo.Sdk.Items;

/// <summary>Default implementation of <see cref="IItemService"/>.</summary>
public sealed class ItemService : IItemService
{
    public ItemService(IItemHttpClient client, IFargoEventHub hub)
    {
        this.client = client;

        hub.On<Guid>("OnItemUpdated", guid =>
        {
            if (tracked.TryGetValue(guid, out var item))
            {
                item.RaiseUpdated();
            }
        });

        hub.On<Guid>("OnItemDeleted", guid =>
        {
            if (tracked.TryGetValue(guid, out var item))
            {
                item.RaiseDeleted();
            }
        });

        this.hub = hub;
    }

    private readonly Dictionary<Guid, Item> tracked = new();
    private readonly IItemHttpClient client;
    private readonly IFargoEventHub hub;

    public async Task<Item> GetAsync(Guid itemGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync(itemGuid, temporalAsOf, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        return await ToEntityAsync(response.Data!);
    }

    public async Task<IReadOnlyCollection<Item>> GetManyAsync(Guid? articleGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, Guid? partitionGuid = null, bool? noPartition = null, CancellationToken cancellationToken = default)
    {
        var response = await client.GetManyAsync(articleGuid, temporalAsOf, page, limit, partitionGuid, noPartition, cancellationToken);

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

    public async Task<Item> CreateAsync(Guid articleGuid, Guid? firstPartition = null, CancellationToken cancellationToken = default)
    {
        var response = await client.CreateAsync(articleGuid, firstPartition, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        var item = new Item(response.Data, articleGuid, client, MakeDisposeCallback(response.Data));
        tracked[item.Guid] = item;
        await hub.InvokeAsync("SubscribeToEntityAsync", item.Guid);
        return item;
    }

    public async Task DeleteAsync(Guid itemGuid, CancellationToken cancellationToken = default)
    {
        var response = await client.DeleteAsync(itemGuid, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }
    }

    private async Task<Item> ToEntityAsync(ItemResult r)
    {
        var item = new Item(r.Guid, r.ArticleGuid, client, MakeDisposeCallback(r.Guid), r.EditedByGuid);
        tracked[item.Guid] = item;
        await hub.InvokeAsync("SubscribeToEntityAsync", item.Guid);
        return item;
    }

    private Func<ValueTask> MakeDisposeCallback(Guid guid) => async () =>
    {
        tracked.Remove(guid);
        await hub.InvokeAsync("UnsubscribeFromEntityAsync", guid);
    };

    private static void ThrowError(FargoSdkError error) =>
        throw new FargoSdkApiException(error.Detail);
}
