using Fargo.Application.Exceptions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Services.PartitionServices;
using Fargo.Domain.Services.UserServices;

namespace Fargo.Application.Requests.Commands.PartitionCommands
{
    public sealed record PartitionDeleteCommand(
            Guid PartitionGuid
            ) : ICommand;

    public sealed class PartitionDeleteCommandHandler(
            PartitionDeleteService partitionDeleteService,
            PartitionGetService partitionGetService,
            ActorGetService actorGetService,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<PartitionDeleteCommand>
    {
        public async Task Handle(
                PartitionDeleteCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = await actorGetService.GetActor(
                    currentUser.UserGuid,
                    cancellationToken
                    ) ?? throw new UnauthorizedAccessFargoApplicationException();

            var partition = await partitionGetService.GetPartition(
                    actor,
                    command.PartitionGuid,
                    cancellationToken
                    ) ?? throw new PartitionNotFoundException(command.PartitionGuid);

            partitionDeleteService.DeletePartition(actor, partition);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}