using Fargo.Domain.Entities.Itens;
using Fargo.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories;

public class ItemRepository(FargoContext fagoContext) : IItemRepository
{
    private readonly FargoContext fargoContext = fagoContext;

    public void Add(Item item)
    {
        fargoContext.Items.Add(item);
    }

    public async Task<Item?> GetByGuidAsync(Guid guid)
    {
        return await fargoContext.Items.FirstOrDefaultAsync(i => i.Guid == guid);
    }

    public void Remove(Item item)
    {
        fargoContext.Items.Remove(item);
    }
}
