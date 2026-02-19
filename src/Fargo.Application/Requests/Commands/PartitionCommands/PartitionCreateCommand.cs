using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.PartitionCommands
{
    public sealed record PartitionCreateCommand(PartitionCreateModel Partition) : ICommand<Guid>;

    public sealed class PartitionCreateCommandHandler(
        PartitionService service,
        IUnitOfWork unitOfWork
        ) : ICommandHandler<PartitionCreateCommand, Guid>
    {
        public async Task<Guid> Handle(PartitionCreateCommand command, CancellationToken cancellationToken = default)
        {
            var partition = service.CreatePartition(
                command.Partition.Name,
                command.Partition.Description ?? default
                );

            await unitOfWork.SaveChanges(cancellationToken);

            return partition.Guid;
        }
    }
}