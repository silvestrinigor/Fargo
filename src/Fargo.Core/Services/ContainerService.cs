using Fargo.Core.Contracts;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Services;

public class ContainerService(IContainerRepository containerRepository)
{
    private readonly IContainerRepository containerRepository = containerRepository;

    public async Task InsertEntityIntoContainer(Container container, Entity entity)
    {
        var fromContainer = await containerRepository.GetEntityContainer(entity.Guid);
        fromContainer?.entities.Remove(entity);
        container.entities.Add(entity);
    }
    public void RemoveEntityFromContainer(Container container, Entity entity)
    {
        container.entities.Remove(entity);
    }
}
