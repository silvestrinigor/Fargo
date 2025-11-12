using Fargo.Application.Dtos;

namespace Fargo.Application.Contracts.Http;

public interface IPartitionHttpClientService
{
    Task<EntityDto?> GetPartitionAsync(Guid guid);
    Task<IEnumerable<EntityDto>> GetPartitionAsync();
    Task<IEnumerable<Guid>> GetPartitionGuidsAsync();
    Task<EntityDto> CreatePartitoinAsync(EntityCreateDto partitionCreateDto);
    Task DeletePartitionAsync(Guid guid);
    Task UpdatePartitionAsync(Guid partitionGuid, EntityUpdateDto partitionUpdateDto);
    Task<IEnumerable<EntityDto>> GetPartitionEntitiesAsync(Guid partitionGuid);
    Task InsertEntityIntoPartitionAsync(Guid partitionGuid, Guid entityGuid);
    Task RemoveEntityFromPartitionAsync(Guid partitionGuid, Guid entityGuid);
}
