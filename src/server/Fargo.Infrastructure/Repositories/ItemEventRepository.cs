using Fargo.Application.Items;
using Fargo.Core.Items.Events;
using Fargo.Infrastructure.Persistence;

namespace Fargo.Infrastructure.Repositories;

public sealed class ItemEventRepository(FargoDbContext context) : IItemEventRepository
{
    public void Add(ItemMovimentEvent itemMovimentEvent)
    {
        context.ItemMovimentEvents.Add(itemMovimentEvent);
    }

    public async Task<IReadOnlyCollection<ItemContainerInventoryDto>> GetEventInventoryInfoByItemContainerGuidAtInstantAsync(
        Guid itemContainerGuid,
        DateTimeOffset at,
        IReadOnlyCollection<Guid>? articleGuids = null,
        bool includeDescendents = true,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
