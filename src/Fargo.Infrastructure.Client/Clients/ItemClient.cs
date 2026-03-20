using Fargo.Application.Models.ItemModels;
using Fargo.Domain.ValueObjects;
using Fargo.HttpApi.Client.Contracts;
using Fargo.Infrastructure.Client.Http;

namespace Fargo.Infrastructure.Client.Clients;

public sealed class ItemClient(HttpClient http)
    : FargoHttpClientBase(http), IItemClient
{
    public Task<ItemInformation?> GetSingleAsync(
        Guid itemGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
    {
        var uri = $"/items/{itemGuid}?temporalAsOf={temporalAsOf}";
        return GetAsync<ItemInformation?>(uri, cancellationToken);
    }

    public Task<IReadOnlyCollection<ItemInformation>> GetManyAsync(
        Guid? articleGuid = null,
        DateTimeOffset? temporalAsOf = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken cancellationToken = default)
    {
        var uri =
            $"/items?articleGuid={articleGuid}&temporalAsOf={temporalAsOf}&page={page}&limit={limit}";

        return GetCollectionAsync<ItemInformation>(uri, cancellationToken);
    }

    public Task<Guid> CreateAsync(
        ItemCreateModel model,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<Guid>("/items", model, cancellationToken);
    }

    public Task UpdateAsync(
        Guid itemGuid,
        ItemUpdateModel model,
        CancellationToken cancellationToken = default)
    {
        return PatchAsync($"/items/{itemGuid}", model, cancellationToken);
    }

    public Task DeleteAsync(
        Guid itemGuid,
        CancellationToken cancellationToken = default)
    {
        return base.DeleteAsync($"/items/{itemGuid}", cancellationToken);
    }
}
