using Fargo.Application.Dtos;

namespace Fargo.Application.Contracts;

public interface IContainerApplicationService
{
    Task<EntityDto?> GetContainerAsync(Guid guid);
    Task<IEnumerable<EntityDto>> GetContainerAsync();
    Task<EntityDto> CreateContainerAsync(EntityDto articleCreateDto);
    Task DeleteContainerAsync(Guid guid);
    Task UpdateContainerAsync(EntityDto articleUpdateDto);
    Task InsertEntityIntoContainer(Guid containerGuid, Guid entityGuid);
}
