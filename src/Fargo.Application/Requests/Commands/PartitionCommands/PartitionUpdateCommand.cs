using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.PartitionCommands
{
    public sealed record PartitionUpdateCommand(
        Guid PartitionGuid,
        PartitionUpdateModel Partition
        ) : ICommand;

    public sealed class PartitionUpdateCommandHandler(
        PartitionService service,
        IUnitOfWork unitOfWork
        ) : ICommandHandlerAsync<PartitionUpdateCommand>
    {
        private readonly PartitionService service = service;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task HandleAsync(PartitionUpdateCommand command, CancellationToken cancellationToken = default)
        {
            var partition = await service.GetPartitionAsync(command.PartitionGuid, cancellationToken);

            partition.Name = command.Partition.Name ?? partition.Name;

            partition.Description = command.Partition.Description ?? partition.Description;

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
