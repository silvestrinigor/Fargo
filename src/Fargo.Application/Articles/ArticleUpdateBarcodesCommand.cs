using Fargo.Application.Authentication;
using Fargo.Application.Events;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.Articles;
using Fargo.Domain.Barcodes;
using Fargo.Domain.Events;

namespace Fargo.Application.Articles;

/// <summary>
/// Command used to replace all barcodes associated with an article.
/// </summary>
/// <param name="ArticleGuid">The unique identifier of the article.</param>
/// <param name="Barcodes">The desired barcode state.</param>
public sealed record ArticleUpdateBarcodesCommand(
    Guid ArticleGuid,
    ArticleBarcodes Barcodes
    ) : ICommand;

/// <summary>
/// Handles <see cref="ArticleUpdateBarcodesCommand"/>.
/// </summary>
public sealed class ArticleUpdateBarcodesCommandHandler(
    ActorService actorService,
    IArticleRepository articleRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IEventRecorder eventRecorder,
    IFargoEventPublisher eventPublisher
    ) : ICommandHandler<ArticleUpdateBarcodesCommand>
{
    /// <summary>
    /// Executes the command to replace all barcodes associated with an article.
    /// </summary>
    /// <param name="command">The command containing article and barcode data.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public async Task Handle(
        ArticleUpdateBarcodesCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(command.Barcodes);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        article.ReplaceBarcodes(command.Barcodes);

        await eventRecorder.Record(EventType.ArticleUpdated, EntityType.Article, article.Guid, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);
        await eventPublisher.PublishArticleUpdated(article.Guid, cancellationToken);
    }
}
