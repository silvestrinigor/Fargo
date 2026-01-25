using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.PartitionCommands
{
    public sealed record PartitionDeleteCommand(
        Guid PartitionGuid
        ) : ICommand<Task>;

    public sealed class PartitionDeleteCommandHandler(
        PartitionService service,
        IUnitOfWork unitOfWork
        ) : ICommandHandler<PartitionDeleteCommand, Task>
    {
        private readonly PartitionService service = service;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task Handle(PartitionDeleteCommand command, CancellationToken cancellationToken = default)
        {
            var partitionToDelete = await service.GetPartitionAsync(command.PartitionGuid, cancellationToken);

            service.DeletePartition(partitionToDelete);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}