using Fargo.Application.Contracts;
using Fargo.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fargo.Application.Services;

public class PartitionApplicationHttpClientService : IPartitionApplicationService
{
    public Task<EntityDto> CreatePartitoinAsync(EntityCreateDto partitionCreateDto)
    {
        throw new NotImplementedException();
    }

    public Task DeletePartitionAsync(Guid guid)
    {
        throw new NotImplementedException();
    }

    public Task<EntityDto?> GetPartitionAsync(Guid guid)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<EntityDto>> GetPartitionAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<EntityDto>> GetPartitionEntitiesAsync(Guid partitionGuid)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Guid>> GetPartitionGuidsAsync()
    {
        throw new NotImplementedException();
    }

    public Task InsertEntityIntoPartitionAsync(Guid partitionGuid, Guid entityGuid)
    {
        throw new NotImplementedException();
    }

    public Task RemoveEntityFromPartitionAsync(Guid partitionGuid, Guid entityGuid)
    {
        throw new NotImplementedException();
    }

    public Task UpdatePartitionAsync(Guid partitionGuid, EntityUpdateDto partitionUpdateDto)
    {
        throw new NotImplementedException();
    }
}
