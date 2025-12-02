using Fargo.Application.Solicitations.Commands.ContainerCommands;
using Fargo.Application.Solicitations.Queries.ContainerQueries;
using Fargo.Application.Solicitations.Responses;

namespace Fargo.Application.Interfaces.Services
{
    public interface IContainerService
    {
        Task<Guid> CreateContainerAsync(ContainerCreateCommand command);
        Task<ContainerInformation?> GetContainerAsync(ContainerQuery getContainerQuery);
        Task AddEntityInContainerAsync(ContainerItemAddCommand command);
        Task<IEnumerable<Guid>> GetChildEntitiesGuids(ContainerChildEntitiesGuidQuery query);
        Task DeleteContainerAsync(ContainerDeleteCommand command);
    }
}
