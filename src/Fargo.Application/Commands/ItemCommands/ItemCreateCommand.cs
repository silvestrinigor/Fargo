using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Helpers;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands.ItemCommands;

/// <summary>
/// Command used to create a new <see cref="Item"/>.
/// </summary>
/// <param name="Item">
/// The data required to create the item.
/// </param>
public sealed record ItemCreateCommand(
        ItemCreateModel Item
        ) : ICommand<Guid>;

/// <summary>
/// Handles the execution of <see cref="ItemCreateCommand"/>.
/// </summary>
public sealed class ItemCreateCommandHandler(
        IItemRepository itemRepository,
        IArticleRepository articleRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<ItemCreateCommand, Guid>
{
    /// <summary>
    /// Executes the command to create a new item.
    /// </summary>
    /// <param name="command">The command containing the item creation data.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The unique identifier of the created item.</returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current user cannot be resolved.
    /// </exception>
    /// <exception cref="ArticleNotFoundFargoApplicationException">
    /// Thrown when the specified article does not exist.
    /// </exception>
    public async Task<Guid> Handle(
            ItemCreateCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await userRepository.GetActiveActor(currentUser, cancellationToken);

        UserPermissionHelper.ValidatePermission(actor, ActionType.CreateItem);

        var article = await articleRepository.GetByGuid(
                command.Item.ArticleGuid,
                cancellationToken
                ) ?? throw new ArticleNotFoundFargoApplicationException(
                        command.Item.ArticleGuid);

        var item = new Item
        {
            Article = article
        };

        itemRepository.Add(item);

        await unitOfWork.SaveChanges(cancellationToken);

        return item.Guid;
    }
}
