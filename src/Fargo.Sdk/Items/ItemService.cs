using System.Collections.Concurrent;
using Fargo.Sdk.Events;

namespace Fargo.Sdk.Items;

/// <summary>Default implementation of <see cref="IItemService"/>.</summary>
public sealed class ItemService : IItemService
{
    /// <summary>Initializes a new instance.</summary>
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

    private readonly ConcurrentDictionary<Guid, Item> tracked = new();
    private readonly IItemHttpClient client;
    private readonly IFargoEventHub hub;

    /// <inheritdoc />
    public async Task<Item> GetAsync(Guid itemGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync(itemGuid, temporalAsOf, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        return await ToEntityAsync(response.Data!);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<Item> CreateAsync(Guid articleGuid, Guid? firstPartition = null, DateTimeOffset? productionDate = null, CancellationToken cancellationToken = default)
    {
        var response = await client.CreateAsync(articleGuid, firstPartition, productionDate, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        var item = new Item(response.Data, articleGuid, client, MakeDisposeCallback(response.Data), productionDate);
        return await TrackAsync(item);
    }

    /// <inheritdoc />
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
        var item = new Item(r.Guid, r.ArticleGuid, client, MakeDisposeCallback(r.Guid), r.ProductionDate, r.EditedByGuid);
        return await TrackAsync(item);
    }

    private Func<ValueTask> MakeDisposeCallback(Guid guid) => async () =>
    {
        tracked.TryRemove(guid, out _);
        await hub.InvokeAsync("UnsubscribeFromEntityAsync", guid);
    };

    private async Task<Item> TrackAsync(Item item)
    {
        var trackedItem = tracked.GetOrAdd(item.Guid, item);

        if (ReferenceEquals(trackedItem, item))
        {
            await hub.InvokeAsync("SubscribeToEntityAsync", item.Guid);
        }

        return trackedItem;
    }

    private static void ThrowError(FargoSdkError error) =>
        throw new FargoSdkApiException(error.Detail);
}
