using Fargo.Application;
using Fargo.Application.Shared.Items;

namespace Fargo.HttpApi.Client.Items;

public sealed class ItemHttpApiClient : IItemHttpApiClient
{
    public Task<Guid> CreateAsync(ItemCreateDto request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid itemGuid, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ItemDto?> GetAsync(Guid itemGuid, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<ItemDto>> GetManyAsync(Pagination? pagination = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Guid itemGuid, ItemUpdateDto request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
