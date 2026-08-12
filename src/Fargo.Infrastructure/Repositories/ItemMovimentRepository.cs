using Fargo.Core.Items;
using Fargo.Infrastructure.Persistence;

namespace Fargo.Infrastructure.Repositories;

public sealed class ItemMovimentRepository(FargoDbContext context) : IItemMovimentRepository
{
    public void Add(ItemMoviment itemMoviment)
    {
        context.ItemMoviments.Add(itemMoviment);
    }
}
