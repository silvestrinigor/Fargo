using Fargo.Domain.Entities;
using Fargo.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories;

public class ContainerRepository(FargoContext fargoContext) : IContainerRepository
{
    private readonly FargoContext fargoContext = fargoContext;

    public void Add(Container container)
    {
        fargoContext.Containers.Add(container);
    }

    public async Task<Container?> GetByGuidAsync(Guid guid)
    {
        return await fargoContext.Containers
            .FirstOrDefaultAsync(c => c.Guid == guid);
    }

    public void Remove(Container container)
    {
        fargoContext.Containers.Remove(container);
    }
}
