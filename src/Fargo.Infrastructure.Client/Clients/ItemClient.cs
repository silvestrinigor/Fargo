using Fargo.Application.Common;
using Fargo.Application.Models.ItemModels;
using Fargo.HttpApi.Client.Interfaces;
using Fargo.Infrastructure.Client.Http;

namespace Fargo.Infrastructure.Client.Clients;

public sealed class ItemClient(HttpClient http)
    : FargoHttpClientBase(http), IItemClient
{
    public Task<ItemReadModel?> GetSingleAsync(
        Guid itemGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken ct = default)
        => GetAsync<ItemReadModel>($"/items/{itemGuid}?temporalAsOf={temporalAsOf}", ct);

    public Task<IReadOnlyCollection<ItemReadModel>> GetManyAsync(
        Guid? articleGuid = null,
        DateTimeOffset? temporalAsOf = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken ct = default)
    {
        var uri =
            $"/items?articleGuid={articleGuid}&temporalAsOf={temporalAsOf}&page={page}&limit={limit}";

        return GetCollectionAsync<ItemReadModel>(uri, ct);
    }

    public Task<Guid> CreateAsync(ItemCreateModel model, CancellationToken ct = default)
        => PostAsync<Guid>("/items", model, ct);

    public Task UpdateAsync(Guid itemGuid, ItemUpdateModel model, CancellationToken ct = default)
        => PatchAsync($"/items/{itemGuid}", model, ct);

    public Task DeleteAsync(Guid itemGuid, CancellationToken ct = default)
        => DeleteAsync($"/items/{itemGuid}", ct);
}