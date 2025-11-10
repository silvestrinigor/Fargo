using Fargo.Core.Contracts;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Abstracts;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories;

public class ContainerRepository(FargoContext fargoContext) : IContainerRepository
{
    private readonly FargoContext fargoContext = fargoContext;

    public async Task<Container?> GetAsync(Guid guid)
        => await fargoContext.Containers.Where(x => x.Guid == guid).FirstOrDefaultAsync();

    public async Task<IEnumerable<Container>> GetAsync()
        => await fargoContext.Containers.ToListAsync();

    public async Task<Container?> GetEntityContainer(Guid entityGuid)
        => await fargoContext.Containers.Where(c => c.Entities.Any(e => e.Guid == entityGuid)).FirstOrDefaultAsync();

    public async Task<IEnumerable<Guid>> GetGuidsAsync()
        => await fargoContext.Containers.Select(x => x.Guid).ToListAsync();

    public void Remove(Container container)
        => fargoContext.Containers.Remove(container);

    public void Add(Container container)
        => fargoContext.Containers.Add(container);

    public async Task<IEnumerable<Entity>> GetContainerEntities(Guid containerGuid)
        => await fargoContext.Containers
            .Where(x => x.Guid == containerGuid)
            .SelectMany(x => x.Entities)
            .ToListAsync();
}
