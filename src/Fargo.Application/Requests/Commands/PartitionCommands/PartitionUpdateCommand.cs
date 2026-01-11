using Fargo.Application.Mediators;
using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Persistence;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands.PartitionCommands
{
    public sealed record PartitionUpdateCommand(
        Guid PartitionGuid,
        PartitionUpdateModel Partition
        ) : ICommand;

    public sealed class PartitionUpdateCommandHandler(
        IPartitionRepository repository, IUnitOfWork unitOfWork
        ) : ICommandHandlerAsync<PartitionUpdateCommand>
    {
        private readonly IPartitionRepository repository = repository;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task HandleAsync(PartitionUpdateCommand command, CancellationToken cancellationToken = default)
        {
            var partition = await repository.GetByGuidAsync(command.PartitionGuid, cancellationToken)
                ?? throw new InvalidOperationException("Partition not found.");

            partition.Name = command.Partition.Name ?? partition.Name;

            partition.Description = command.Partition.Description ?? partition.Description;

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
