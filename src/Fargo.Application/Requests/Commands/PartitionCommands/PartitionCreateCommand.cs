using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.PartitionCommands
{
    public sealed record PartitionCreateCommand(PartitionCreateModel Partition) : ICommand<Guid>;

    public sealed class PartitionCreateCommandHandler(
        PartitionService service,
        IUnitOfWork unitOfWork
        ) : ICommandHandlerAsync<PartitionCreateCommand, Guid>
    {
        private readonly PartitionService service = service;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task<Guid> HandleAsync(PartitionCreateCommand command, CancellationToken cancellationToken = default)
        {
            var partition = service.CreatePartition(
                command.Partition.Name,
                command.Partition.Description ?? default);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return partition.Guid;
        }
    }
}