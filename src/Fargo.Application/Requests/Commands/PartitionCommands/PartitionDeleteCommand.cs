using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Repositories.PartitionRepositories;

namespace Fargo.Application.Requests.Commands.PartitionCommands
{
    public sealed record PartitionDeleteCommand(
        Guid PartitionGuid
        ) : ICommand;

    public sealed class PartitionDeleteCommandHandler(IPartitionRepository repository, IUnitOfWork unitOfWork) : ICommandHandlerAsync<PartitionDeleteCommand>
    {
        private readonly IPartitionRepository repository = repository;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task HandleAsync(PartitionDeleteCommand command, CancellationToken cancellationToken = default)
        {
            var partitionToDelete = await repository.GetByGuidAsync(command.PartitionGuid, cancellationToken)
                ?? throw new InvalidOperationException("Partition not found.");

            repository.Remove(partitionToDelete);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
