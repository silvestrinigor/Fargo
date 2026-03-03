using Fargo.Application.Exceptions;
using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands.PartitionCommands
{
    public sealed record PartitionUpdateCommand(
            Guid PartitionGuid,
            PartitionUpdateModel Partition
            ) : ICommand;

    public sealed class PartitionUpdateCommandHandler(
            IPartitionRepository partitionRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<PartitionUpdateCommand>
    {
        public async Task Handle(
                PartitionUpdateCommand command,
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
                    currentUser.UserGuid,
                    [.. actor.PartitionsAccesses.Select(x => x.Guid)],
                    cancellationToken
                    )
                ?? throw new PartitionNotFoundException(command.PartitionGuid);

            partition.Name = command.Partition.Name ?? partition.Name;

            partition.Description = command.Partition.Description ?? partition.Description;

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}