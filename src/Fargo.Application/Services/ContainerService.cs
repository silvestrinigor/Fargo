using Fargo.Application.Interfaces.Persistence;
using Fargo.Application.Interfaces.Services;
using Fargo.Application.Solicitations.Commands.ContainerCommands;
using Fargo.Application.Solicitations.Queries.ContainerQueries;
using Fargo.Application.Solicitations.Responses;
using Fargo.Domain.Entities;
using Fargo.Domain.Interfaces.Repositories;

namespace Fargo.Application.Services
{
    public class ContainerService(IContainerRepository containerRepository, IEntityRepository entityRepository, IUnitOfWork unitOfWork) : IContainerService
    {
        private readonly IContainerRepository containerRepository = containerRepository;
        private readonly IEntityRepository entityRepository = entityRepository;
        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task<Guid> CreateContainerAsync(ContainerCreateCommand command)
        {
            var container = new Container { Name = command.Name };

            containerRepository.Add(container);

            await unitOfWork.SaveChangesAsync();

            return container.Guid;
        }

        public async Task<ContainerInformation?> GetContainerAsync(ContainerQuery getContainerQuery)
        {
            var container = await containerRepository.GetByGuidAsync(getContainerQuery.ContainerGuid);

            if (container == null)
            {
                return null;
            }

            return new ContainerInformation(
                container.Guid,
                container.Name,
                container.Description,
                container.CreatedAt,
                container.ParentGuid
            );
        }

        public async Task AddEntityInContainerAsync(ContainerItemAddCommand command)
        {
            var entity = await entityRepository.GetByGuidAsync(command.EntityGuid)
                ?? throw new InvalidOperationException("Entity not found.");

            var container = await containerRepository.GetByGuidAsync(command.ContainerGuid)
                ?? throw new InvalidOperationException("Container not found.");

            container.Add(entity);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<Guid>> GetChildEntitiesGuids(ContainerChildEntitiesGuidQuery query)
        {
            return await containerRepository.GetChildEntitiesGuidsAsync(query.ContainerGuid);
        }

        public async Task DeleteContainerAsync(ContainerDeleteCommand command)
        {
            var container = await containerRepository.GetByGuidAsync(command.EntityGuid)
                ?? throw new InvalidOperationException("Container not found.");

            var hasChildEntities = await containerRepository.HasChildEntitiesAsync(container.Guid);

            if (hasChildEntities)
            {
                throw new InvalidOperationException("Cannot delete container with child entities.");
            }

            containerRepository.Remove(container);

            await unitOfWork.SaveChangesAsync();
        }
    }
}
