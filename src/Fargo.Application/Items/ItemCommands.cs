using Fargo.Application.Articles;
using Fargo.Application.Authentication;
using Fargo.Application.Partitions;
using Fargo.Domain;
using Fargo.Domain.Articles;
using Fargo.Domain.Items;
using Fargo.Domain.Partitions;

namespace Fargo.Application.Items;

#region Create

public sealed record ItemCreateCommand(
    ItemCreateDto Item
) : ICommand<Guid>;

public sealed class ItemCreateCommandHandler(
    ActorService actorService,
    IItemRepository itemRepository,
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork
) : ICommandHandler<ItemCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        ItemCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.CreateItem);

        var article = await articleRepository.GetFoundByGuid(command.Item.ArticleGuid, cancellationToken);

        var item = new Item
        {
            Article = article,
            ProductionDate = command.Item.ProductionDate
        };

        #region Partition

        foreach (var partitionGuid in command.Item.Partitions ?? [])
        {
            var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

            actor.ValidateHasPartitionAccess(partition.Guid);

            item.Partitions.Add(partition);
        }

        #endregion Partition

        itemRepository.Add(item);

        await unitOfWork.SaveChanges(cancellationToken);

        return item.Guid;
    }
}

#endregion Create

#region Delete

public sealed record ItemDeleteCommand(
    Guid ItemGuid
) : ICommand;

public sealed class ItemDeleteCommandHandler(
    ActorService actorService,
    IItemRepository itemRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
) : ICommandHandler<ItemDeleteCommand>
{
    public async Task Handle(
        ItemDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.DeleteItem);

        var item = await itemRepository.GetFoundByGuid(command.ItemGuid, cancellationToken);

        actor.ValidateHasAccess(item);

        itemRepository.Remove(item);

        await unitOfWork.SaveChanges(cancellationToken);
    }
}

#endregion Delete

#region Update

public sealed record ItemUpdateCommand(
    Guid ItemGuid,
    ItemUpdateDto Item
) : ICommand;

public sealed class ItemUpdateCommandHandler(
    ActorService actorService,
    IItemRepository itemRepository,
    IPartitionRepository partitionRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
) : ICommandHandler<ItemUpdateCommand>
{
    public async Task Handle(
        ItemUpdateCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditItem);

        var item = await itemRepository.GetFoundByGuid(command.ItemGuid, cancellationToken);

        actor.ValidateHasAccess(item);

        #region Partition

        if (command.Item.Partitions is { } requestedPartitions)
        {
            foreach (var partitionGuid in requestedPartitions)
            {
                if (item.Partitions.Any(p => p.Guid == partitionGuid))
                {
                    continue;
                }

                var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

                actor.ValidateHasPartitionAccess(partition.Guid);

                item.Partitions.Add(partition);
            }

            var partitionsToRemove = item.Partitions
                .Where(p => !requestedPartitions.Contains(p.Guid))
                .ToList();

            foreach (var partition in partitionsToRemove)
            {
                actor.ValidateHasPartitionAccess(partition.Guid);

                item.Partitions.Remove(partition);
            }
        }

        #endregion Partition

        await unitOfWork.SaveChanges(cancellationToken);
    }
}

#endregion Update
