using Fargo.Application;
using Fargo.Application.Shared.Items;

namespace Fargo.HttpApi.Client.Items;

public interface IItemHttpApiClient
{
    Task<ItemDto?> GetAsync(Guid itemGuid, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ItemDto>> GetManyAsync(Pagination? pagination = null, CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(ItemCreateDto request, CancellationToken cancellationToken = default);

    Task UpdateAsync(Guid itemGuid, ItemUpdateDto request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid itemGuid, CancellationToken cancellationToken = default);
}
