using Fargo.Application.Exceptions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands.PartitionCommands
{
    public sealed record PartitionDeleteCommand(
            Guid PartitionGuid
            ) : ICommand;

    public sealed class PartitionDeleteCommandHandler(
            IPartitionRepository partitionRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<PartitionDeleteCommand>
    {
        public async Task Handle(
                PartitionDeleteCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = await userRepository.GetByGuid(
                    currentUser.UserGuid,
                    partitionGuids: null,
                    cancellationToken
                    )
                ?? throw new UnauthorizedAccessFargoApplicationException();

            var partition = await partitionRepository.GetByGuid(
                    command.PartitionGuid,
                    [.. actor.PartitionsAccesses.Select(x => x.Guid)],
                    cancellationToken
                    )
                ?? throw new PartitionNotFoundException(command.PartitionGuid);

            actor.ValidatePermission(ActionType.DeletePartition);

            partitionRepository.Remove(partition);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}