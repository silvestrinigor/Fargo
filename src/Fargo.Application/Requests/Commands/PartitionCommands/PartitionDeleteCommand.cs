using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.PartitionCommands
{
    public sealed record PartitionDeleteCommand(
        Guid PartitionGuid
        ) : ICommand;

    public sealed class PartitionDeleteCommandHandler(
        PartitionService service,
        IUnitOfWork unitOfWork
        ) : ICommandHandlerAsync<PartitionDeleteCommand>
    {
        private readonly PartitionService service = service;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task HandleAsync(PartitionDeleteCommand command, CancellationToken cancellationToken = default)
        {
            var partitionToDelete = await service.GetPartitionAsync(command.PartitionGuid, cancellationToken);

            service.DeletePartition(partitionToDelete);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
