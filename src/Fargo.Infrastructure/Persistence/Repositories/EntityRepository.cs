using Fargo.Core.Contracts;
using Fargo.Core.Entities.Abstracts;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories;

internal class EntityRepository(FargoContext fagoContext) : IEntityMainRepository
{
    private readonly FargoContext fargoContext = fagoContext;

    public async Task<Entity?> GetAsync(Guid guid)
        => await fargoContext.Entities.Where(x => x.Guid == guid).FirstOrDefaultAsync();

    public async Task<IEnumerable<Entity>> GetAsync()
        => await fargoContext.Entities.ToListAsync();

    public async Task<IEnumerable<Guid>> GetGuidsAsync()
        => await fargoContext.Entities.Select(x => x.Guid).ToListAsync();

    public void Add(Entity article)
        => fargoContext.Entities.Add(article);

    public void Remove(Entity article)
        => fargoContext.Entities.Remove(article);
}
