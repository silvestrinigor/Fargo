using Fargo.HttpContracts;

namespace Fargo.HttpClient;

public interface IFargoItemClient
{
    Task<ItemDto?> GetAsync(
        Guid itemGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ItemDto>> GetManyAsync(
        FargoListQuery? query = null,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(
        ItemCreateRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid itemGuid,
        ItemUpdateRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid itemGuid,
        CancellationToken cancellationToken = default);
}

internal sealed class FargoItemClient(FargoHttpTransport transport) : IFargoItemClient
{
    public Task<ItemDto?> GetAsync(
        Guid itemGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
        => transport.SendNullableAsync<ItemDto>(
            HttpMethod.Get,
            FargoHttpTransport.BuildPath(
                $"/items/{itemGuid:D}",
                FargoHttpTransport.SingleQuery(temporalAsOf)),
            null,
            cancellationToken);

    public Task<IReadOnlyCollection<ItemDto>> GetManyAsync(
        FargoListQuery? query = null,
        CancellationToken cancellationToken = default)
        => transport.SendCollectionAsync<ItemDto>(
            HttpMethod.Get,
            FargoHttpTransport.BuildPath("/items/", FargoHttpTransport.ListQuery(query)),
            null,
            cancellationToken);

    public Task<Guid> CreateAsync(
        ItemCreateRequest request,
        CancellationToken cancellationToken = default)
        => transport.SendRequiredAsync<Guid>(
            HttpMethod.Post,
            "/items/",
            request,
            cancellationToken);

    public Task UpdateAsync(
        Guid itemGuid,
        ItemUpdateRequest request,
        CancellationToken cancellationToken = default)
        => transport.SendNoContentAsync(
            HttpMethod.Put,
            $"/items/{itemGuid:D}",
            request,
            cancellationToken);

    public Task DeleteAsync(
        Guid itemGuid,
        CancellationToken cancellationToken = default)
        => transport.SendNoContentAsync(
            HttpMethod.Delete,
            $"/items/{itemGuid:D}",
            null,
            cancellationToken);
}
