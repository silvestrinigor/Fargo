using Fargo.Application.Extensions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.PartitionCommands
{
    public sealed record PartitionDeleteCommand(
        Guid PartitionGuid
        ) : ICommand;

    public sealed class PartitionDeleteCommandHandler(
        PartitionService service,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<PartitionDeleteCommand>
    {
        public async Task Handle(
                PartitionDeleteCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var partitionToDelete = await service.GetPartitionAsync(
                    currentUser.ToActor(),
                    command.PartitionGuid,
                    cancellationToken
                    );

            service.DeletePartition(partitionToDelete);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}