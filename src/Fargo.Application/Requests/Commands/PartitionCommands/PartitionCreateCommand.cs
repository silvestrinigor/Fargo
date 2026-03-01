using Fargo.Application.Exceptions;
using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Services.PartitionServices;
using Fargo.Domain.Services.UserServices;

namespace Fargo.Application.Requests.Commands.PartitionCommands
{
    public sealed record PartitionCreateCommand(PartitionCreateModel Partition)
        : ICommand<Guid>;

    public sealed class PartitionCreateCommandHandler(
            PartitionCreateService partitionCreateService,
            ActorGetService actorGetService,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork
            ) : ICommandHandler<PartitionCreateCommand, Guid>
    {
        public async Task<Guid> Handle(
                PartitionCreateCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = await actorGetService.GetActor(
                    currentUser.UserGuid,
                    cancellationToken
                    ) ?? throw new UnauthorizedAccessFargoApplicationException();

            var partition = partitionCreateService.CreatePartition(
                    actor,
                    command.Partition.Name
                    );

            await unitOfWork.SaveChanges(cancellationToken);

            return partition.Guid;
        }
    }
}