using Fargo.Application.Exceptions;
using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Services.PartitionServices;
using Fargo.Domain.Services.UserServices;

namespace Fargo.Application.Requests.Commands.PartitionCommands
{
    public sealed record PartitionUpdateCommand(
            Guid PartitionGuid,
            PartitionUpdateModel Partition
            ) : ICommand;

    public sealed class PartitionUpdateCommandHandler(
            PartitionGetService partitionGetService,
            ActorGetService actorGetService,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<PartitionUpdateCommand>
    {
        public async Task Handle(
                PartitionUpdateCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = await actorGetService.GetActor(
                    currentUser.UserGuid,
                    cancellationToken
                    ) ?? throw new UnauthorizedAccessFargoApplicationException();

            var partition = await partitionGetService.GetPartition(
                    actor,
                    currentUser.UserGuid,
                    cancellationToken
                    ) ?? throw new PartitionNotFoundException(command.PartitionGuid);

            partition.Name = command.Partition.Name ?? partition.Name;

            partition.Description = command.Partition.Description ?? partition.Description;

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}