using Fargo.Application.Extensions;
using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.PartitionCommands
{
    public sealed record PartitionUpdateCommand(
        Guid PartitionGuid,
        PartitionUpdateModel Partition
        ) : ICommand;

    public sealed class PartitionUpdateCommandHandler(
        PartitionService service,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<PartitionUpdateCommand>
    {
        public async Task Handle(
                PartitionUpdateCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var partition = await service.GetPartitionAsync(
                    currentUser.ToActor(),
                    command.PartitionGuid,
                    cancellationToken
                    );

            partition.Name = command.Partition.Name ?? partition.Name;

            partition.Description = command.Partition.Description ?? partition.Description;

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}