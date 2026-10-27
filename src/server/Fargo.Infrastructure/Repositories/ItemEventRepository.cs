using Fargo.Core.Items.Events;
using Fargo.Infrastructure.Persistence;

namespace Fargo.Infrastructure.Repositories;

public sealed class ItemEventRepository(FargoDbContext context) : IItemEventRepository
{
    public void Add(ItemMovimentEvent itemMovimentEvent)
    {
        context.ItemMovimentEvents.Add(itemMovimentEvent);
    }
}
