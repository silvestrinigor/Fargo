using Fargo.Application.Articles;
using Fargo.Application.Partitions;
using Fargo.Application.Events;
using Fargo.Application.Persistence;
using Fargo.Application.Authentication;
using Fargo.Domain;
using Fargo.Domain.Articles;
using Fargo.Domain.Items;
using Fargo.Domain.Partitions;

namespace Fargo.Application.Items;

/// <summary>
/// Command used to create a new <see cref="Item"/>.
/// </summary>
/// <param name="Item">
/// The data required to create the item, including the associated article
/// and the initial partition assignment.
/// </param>
/// <remarks>
/// This command represents the intention to create a concrete instance of an article
/// within a specific partition context.
/// </remarks>
public sealed record ItemCreateCommand(
        ItemCreateModel Item
        ) : ICommand<Guid>;

/// <summary>
/// Handles the execution of <see cref="ItemCreateCommand"/>.
/// </summary>
/// <remarks>
/// This handler is responsible for:
/// <list type="bullet">
/// <item><description>Resolving and authorizing the current actor</description></item>
/// <item><description>Validating permission to create items</description></item>
/// <item><description>Validating access to the target partition</description></item>
/// <item><description>Ensuring the referenced article exists</description></item>
/// <item><description>Creating and persisting the new item</description></item>
/// </list>
///
/// Partition behavior:
/// <list type="bullet">
/// <item><description>
/// If <c>FirstPartition</c> is not provided, the item is assigned to the global partition
/// </description></item>
/// <item><description>
/// The actor must have access to the selected partition
/// </description></item>
/// </list>
/// </remarks>
public sealed class ItemCreateCommandHandler(
        ActorService actorService,
        IItemRepository itemRepository,
        IArticleRepository articleRepository,
        IPartitionRepository partitionRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IFargoEventPublisher eventPublisher
        ) : ICommandHandler<ItemCreateCommand, Guid>
{
    /// <summary>
    /// Executes the command to create a new item.
    /// </summary>
    /// <param name="command">The command containing the item creation data.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The unique identifier of the created item.</returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current user cannot be resolved or is not authorized.
    /// </exception>
    /// <exception cref="UserNotAuthorizedFargoApplicationException">
    /// Thrown when the user does not have permission to create items.
    /// </exception>
    /// <exception cref="ArticleNotFoundFargoApplicationException">
    /// Thrown when the specified article does not exist.
    /// </exception>
    /// <exception cref="PartitionNotFoundFargoApplicationException">
    /// Thrown when the specified partition does not exist.
    /// </exception>
    /// <remarks>
    /// Execution flow:
    /// <list type="number">
    /// <item><description>Resolve the current actor</description></item>
    /// <item><description>Validate <see cref="ActionType.CreateItem"/> permission</description></item>
    /// <item><description>Resolve the target partition (or fallback to global)</description></item>
    /// <item><description>Validate partition access</description></item>
    /// <item><description>Resolve the associated article</description></item>
    /// <item><description>Create and persist the item</description></item>
    /// </list>
    /// </remarks>
    public async Task<Guid> Handle(
            ItemCreateCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.CreateItem);

        var partitionGuid = command.Item.FirstPartition ?? PartitionService.GlobalPartitionGuid;

        var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

        actor.ValidateHasPartitionAccess(partition.Guid);

        var article = await articleRepository.GetFoundByGuid(command.Item.ArticleGuid, cancellationToken);

        var item = new Item
        {
            Article = article
        };

        item.Partitions.Add(partition);

        itemRepository.Add(item);

        await unitOfWork.SaveChanges(cancellationToken);

        await eventPublisher.PublishItemCreated(item.Guid, article.Guid, [partition.Guid], cancellationToken);

        return item.Guid;
    }
}
