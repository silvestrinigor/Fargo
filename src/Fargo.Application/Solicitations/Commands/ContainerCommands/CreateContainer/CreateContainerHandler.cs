using Fargo.Application.Interfaces.Persistence;
using Fargo.Application.Interfaces.Solicitations.Commands;
using Fargo.Domain.Entities;
using Fargo.Domain.Interfaces.Repositories;

namespace Fargo.Application.Solicitations.Commands.ContainerCommands.CreateContainer
{
    public sealed class CreateContainerHandler(IContainerRepository containerRepository, IUnitOfWork unitOfWork) : ICommandHandler<CreateContainerCommand, Task>
    {
        private readonly IContainerRepository containerRepository = containerRepository;
        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task Handle(CreateContainerCommand command)
        {
            var container = new Container { Name = command.Name };

            await containerRepository.AddContainerAsync(container);

            await unitOfWork.SaveChangesAsync();
        }
    }
}