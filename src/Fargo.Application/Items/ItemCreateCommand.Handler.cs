using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Items;
using Fargo.Core.Shared;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

public sealed class ItemCreateCommandHandler(
    ActorService actorService,
    IItemRepository itemRepository,
    IArticleRepository articleRepository,
    ICurrentActor currentActor,
    IUnitOfWork unitOfWork,
    ILogger<ItemCreateCommandHandler> logger
) : ICommandHandler<ItemCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        ItemCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var create = command.Create;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item create flow started for article {articleGuid} by actor {ActorGuid}.",
                create.ArticleGuid, currentActor.ActorId);
        }

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        actor.ThrowIfPermissionNotAuthorized(ActionType.CreateItem);

        var article = await articleRepository.GetByGuidAsync(create.ArticleGuid, cancellationToken);

        EntityAssertFound.ThrowNotFoundIfNull(article);

        actor.ThrowIfAccessNotAuthorized(article);

        var item = Item.CreateItem(article, create.ProductionDate);

        item.IsActive = create.IsActive ?? true;

        itemRepository.Add(item);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Item create mutation completed for item {itemGuid} by actor {actorId}. ArticleGuid: {articleGuid}.",
                item.Guid, actor.ActorId, article.Guid);
        }

        return item.Guid;
    }
}
