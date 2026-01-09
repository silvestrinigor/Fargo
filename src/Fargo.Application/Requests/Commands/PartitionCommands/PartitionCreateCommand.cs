using Fargo.Application.Dtos.PartitionDtos;
using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using UnitsNet;

namespace Fargo.Application.Requests.Commands.PartitionCommands
{
    public sealed record PartitionCreateCommand(
        PartitionCreateDto Partition
        ) : ICommand<Guid>;

    public sealed class PartitionCreateCommandHandler(IPartitionRepository repository, IUnitOfWork unitOfWork) : ICommandHandlerAsync<PartitionCreateCommand, Guid>
    {
        private readonly IPartitionRepository repository = repository;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task<Guid> HandleAsync(PartitionCreateCommand command, CancellationToken cancellationToken = default)
        {
            var partition = new Partition
            {
                Name = command.Partition.Name,
                Description = command.Partition.Description is not null ? command.Partition.Description.Value : default
            };

            repository.Add(partition);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return partition.Guid;
        }
    }
}
