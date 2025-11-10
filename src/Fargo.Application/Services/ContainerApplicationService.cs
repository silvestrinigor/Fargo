using Fargo.Application.Contracts;
using Fargo.Application.Contracts.Persistence;
using Fargo.Application.Dtos;
using Fargo.Application.Extensions;
using Fargo.Core.Contracts;
using Fargo.Core.Entities;
using Fargo.Core.Services;

namespace Fargo.Application.Services;

public class ContainerApplicationService(ContainerService containerService, IEntityMainRepository entityMainRepository, IContainerRepository containerRepository, IUnitOfWork unitOfWork) : IContainerApplicationService
{
    private readonly ContainerService containerService = containerService;
    private readonly IEntityMainRepository entityMainRepository = entityMainRepository;
    private readonly IContainerRepository containerRepository = containerRepository;
    private readonly IUnitOfWork unitOfWork = unitOfWork;

    public async Task<EntityDto> CreateContainerAsync(EntityCreateDto articleCreateDto)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(articleCreateDto.Name);

        var container = new Container(articleCreateDto.Name);

        containerRepository.Add(container);

        await unitOfWork.SaveChangesAsync();

        return container.ToContainerDto();
    }

    public async Task DeleteContainerAsync(Guid guid)
    {
        var container = await containerRepository.GetAsync(guid);

        ArgumentNullException.ThrowIfNull(container);

        containerRepository.Remove(container);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task<EntityDto?> GetContainerAsync(Guid guid)
    {
        var container = await containerRepository.GetAsync(guid);

        return container?.ToContainerDto();
    }

    public async Task<IEnumerable<EntityDto>> GetContainerAsync()
    {
        var containers = await containerRepository.GetAsync();

        return containers.Select(container => container.ToContainerDto());
    }

    public async Task<IEnumerable<EntityDto>> GetContainerEntitiesAsync(Guid containerGuid)
    {
        var entities = await containerRepository.GetContainerEntities(containerGuid);

        return entities.Select(x => x.ToEntityDto());
    }

    public async Task<IEnumerable<Guid>> GetContainersGuidsAsync()
    {
        var containersGuids = await containerRepository.GetGuidsAsync();

        return containersGuids;
    }

    public async Task InsertEntityIntoContainer(Guid containerGuid, Guid entityGuid)
    {
        var container = await containerRepository.GetAsync(containerGuid);
        ArgumentNullException.ThrowIfNull(container);

        var entity = await entityMainRepository.GetAsync(entityGuid);
        ArgumentNullException.ThrowIfNull(entity);

        await containerService.InsertEntityIntoContainer(container, entity);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveEntityFromContainer(Guid containerGuid, Guid entityGuid)
    {
        var container = await containerRepository.GetAsync(containerGuid);
        ArgumentNullException.ThrowIfNull(container);

        var entity = await containerRepository.GetEntityContainer(entityGuid);
        ArgumentNullException.ThrowIfNull(entity);

        containerService.RemoveEntityFromContainer(container, entity);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateContainerAsync(Guid containerGuid, EntityUpdateDto articleUpdateDto)
    {
        var container = await containerRepository.GetAsync(containerGuid);

        ArgumentNullException.ThrowIfNull(container);

        container.UpdateEntityProperties(articleUpdateDto);

        await unitOfWork.SaveChangesAsync();
    }
}
