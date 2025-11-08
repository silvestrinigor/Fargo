using Fargo.Core.Contracts;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Services;

public class ContainerService(IContainerRepository containerRepository)
{
    private readonly IContainerRepository containerRepository = containerRepository;

    public void InsertEntityIntoContainer(Container container, Entity entity)
    {
        var fromContainer = containerRepository.GetEntityContainer(entity.Guid);
        fromContainer?.RemoveChildEntity(entity.Guid);
        container.AddChildEntity(entity.Guid);
    }
}
