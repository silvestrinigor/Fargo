using Fargo.Application.Exceptions;
using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands.PartitionCommands
{
    public sealed record PartitionCreateCommand(PartitionCreateModel Partition)
        : ICommand<Guid>;

    public sealed class PartitionCreateCommandHandler(
            IPartitionRepository partitionRepository,
            IUserRepository userRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork
            ) : ICommandHandler<PartitionCreateCommand, Guid>
    {
        public async Task<Guid> Handle(
                PartitionCreateCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = await userRepository.GetByGuid(
                    currentUser.UserGuid,
                    partitionGuids: null,
                    cancellationToken
                    )
                ?? throw new UnauthorizedAccessFargoApplicationException();

            actor.ValidatePermission(ActionType.CreatePartition);

            var partition = new Partition
            {
                Name = command.Partition.Name,
                UpdatedBy = actor
            };

            partitionRepository.Add(partition);

            await unitOfWork.SaveChanges(cancellationToken);

            return partition.Guid;
        }
    }
}