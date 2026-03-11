using Fargo.Application.Exceptions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands.ItemCommands
{
    /// <summary>
    /// Command used to delete an existing item.
    /// </summary>
    /// <param name="ItemGuid">
    /// The unique identifier of the item to delete.
    /// </param>
    public sealed record ItemDeleteCommand(
            Guid ItemGuid
            ) : ICommand;

    /// <summary>
    /// Handles the execution of <see cref="ItemDeleteCommand"/>.
    /// </summary>
    public sealed class ItemDeleteCommandHandler(
            IItemRepository itemRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<ItemDeleteCommand>
    {
        /// <summary>
        /// Executes the command to delete an existing item.
        /// </summary>
        /// <param name="command">The command containing the item identifier.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="UnauthorizedAccessFargoApplicationException">
        /// Thrown when the current user cannot be resolved.
        /// </exception>
        /// <exception cref="ItemNotFoundFargoApplicationException">
        /// Thrown when the specified item does not exist.
        /// </exception>
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

            actor.ValidateIsActive();
            actor.ValidatePermission(ActionType.DeleteItem);

            var item = await itemRepository.GetByGuid(
                    command.ItemGuid,
                    cancellationToken
                    )
                ?? throw new ItemNotFoundFargoApplicationException(command.ItemGuid);

            itemRepository.Remove(item);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}