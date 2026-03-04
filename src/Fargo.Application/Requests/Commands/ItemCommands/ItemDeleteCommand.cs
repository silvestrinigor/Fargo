using Fargo.Application.Exceptions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands.ItemCommands
{
    public sealed record ItemDeleteCommand(
            Guid ItemGuid
            ) : ICommand;

    public sealed class ItemDeleteCommandHandler(
            IItemRepository itemRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<ItemDeleteCommand>
    {
        public async Task Handle(
                ItemDeleteCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = await userRepository.GetByGuid(
                    currentUser.UserGuid,
                    cancellationToken
                    )
                ?? throw new UnauthorizedAccessFargoApplicationException();

            var item = await itemRepository.GetByGuid(
                    command.ItemGuid,
                    cancellationToken
                    )
                ?? throw new ItemNotFoundFargoApplicationException(command.ItemGuid);

            actor.ValidatePermission(ActionType.DeleteItem);

            itemRepository.Remove(item);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}