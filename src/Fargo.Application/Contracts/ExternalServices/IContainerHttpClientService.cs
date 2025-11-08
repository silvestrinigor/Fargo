using Fargo.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fargo.Application.Contracts.ExternalServices;

public interface IContainerHttpClientService
{
    Task<EntityDto?> GetContainerAsync(Guid guid);
    Task<IEnumerable<EntityDto>> GetContainerAsync();
    Task<EntityDto> CreateContainerAsync(EntityDto articleCreateDto);
    Task DeleteContainerAsync(Guid guid);
    Task UpdateContainerAsync(EntityDto articleUpdateDto);
    Task InsertEntityIntoContainer(Guid containerGuid, Guid entityGuid);
}
