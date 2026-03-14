using Fargo.Application.Models.ItemModels;
using Fargo.Domain.ValueObjects;
using Fargo.Domain.ValueObjects.Entities;

namespace Fargo.HttpApi.Client.Contracts
{
    public interface IItemClient
    {
        Task<ItemInformation?> GetSingleAsync(
            Guid itemGuid,
            DateTimeOffset? temporalAsOf = null,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<ItemInformation>> GetManyAsync(
            Guid? articleGuid = null,
            DateTimeOffset? temporalAsOf = null,
            Page? page = null,
            Limit? limit = null,
            CancellationToken cancellationToken = default);

        Task<Guid> CreateAsync(
            ItemCreateModel model,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            Guid itemGuid,
            ItemUpdateModel model,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            Guid itemGuid,
            CancellationToken cancellationToken = default);
    }
}