using Fargo.Core.Contracts;
using Fargo.Core.Entities;
using Fargo.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public class ContainerRepository(FargoContext fargoContext) : IContainerRepository
{
    private readonly FargoContext fargoContext = fargoContext;

    public void Add(Container container)
    {
        fargoContext.Containers.Add(container);
    }

    public async Task<Container?> GetAsync(Guid guid)
    {
        return await fargoContext.Containers.Where(x => x.Guid == guid).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Container>> GetAsync()
    {
        return await fargoContext.Containers.ToListAsync();
    }

    public async Task<Container?> GetEntityContainer(Guid guid)
    {
        return await fargoContext.Containers.Where(x => x.ChildEntities.Contains(guid)).FirstAsync();
    }

    public void Remove(Container container)
    {
        fargoContext.Containers.Remove(container);
    }
}
