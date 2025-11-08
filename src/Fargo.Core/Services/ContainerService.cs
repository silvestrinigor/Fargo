using Fargo.Core.Contracts;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Abstracts;
using System.Threading.Tasks;

namespace Fargo.Core.Services;

public class ContainerService(IContainerRepository containerRepository)
{
    private readonly IContainerRepository containerRepository = containerRepository;

    public async Task InsertEntityIntoContainer(Container container, Entity entity)
    {
        var fromContainer = await containerRepository.GetEntityContainer(entity.Guid);
        fromContainer?.RemoveChildEntity(entity.Guid);
        container.AddChildEntity(entity.Guid);
    }
}
