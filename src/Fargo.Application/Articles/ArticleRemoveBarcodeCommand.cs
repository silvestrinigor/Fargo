using Fargo.Application.Authentication;
using Fargo.Application.Events;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.Articles;
using Fargo.Domain.Barcodes;
using Fargo.Domain.Events;

namespace Fargo.Application.Articles;

/// <summary>
/// Command used to remove a barcode from an article.
/// </summary>
/// <param name="ArticleGuid">The unique identifier of the article.</param>
/// <param name="BarcodeGuid">The unique identifier of the barcode to remove.</param>
public sealed record ArticleRemoveBarcodeCommand(
    Guid ArticleGuid,
    Guid BarcodeGuid
    ) : ICommand;

/// <summary>
/// Handles <see cref="ArticleRemoveBarcodeCommand"/>.
/// </summary>
public sealed class ArticleRemoveBarcodeCommandHandler(
    ActorService actorService,
    IArticleRepository articleRepository,
    IBarcodeRepository barcodeRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IEventRecorder eventRecorder,
    IFargoEventPublisher eventPublisher
    ) : ICommandHandler<ArticleRemoveBarcodeCommand>
{
    /// <summary>
    /// Executes the command to remove a barcode from an article.
    /// </summary>
    /// <param name="command">The command containing the article and barcode identifiers.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current actor is not authenticated or inactive.
    /// </exception>
    /// <exception cref="UserNotAuthorizedFargoApplicationException">
    /// Thrown when the actor does not have <see cref="ActionType.RemoveBarcode"/> permission.
    /// </exception>
    /// <exception cref="ArticleNotFoundFargoApplicationException">
    /// Thrown when the article does not exist or is not accessible.
    /// </exception>
    /// <exception cref="BarcodeNotFoundFargoApplicationException">
    /// Thrown when the barcode does not exist or does not belong to the article.
    /// </exception>
    public async Task Handle(
        ArticleRemoveBarcodeCommand command,
        CancellationToken cancellationToken = default
        )
    {
        ArgumentNullException.ThrowIfNull(command);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.RemoveBarcode);

        // Ensure the article exists and is accessible.
        await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        var barcode = await barcodeRepository.GetFoundByGuid(command.BarcodeGuid, cancellationToken);

        if (barcode.ArticleGuid != command.ArticleGuid)
        {
            throw new BarcodeNotFoundFargoApplicationException(command.BarcodeGuid);
        }

        barcodeRepository.Remove(barcode);

        await eventRecorder.Record(EventType.ArticleUpdated, EntityType.Article, command.ArticleGuid, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);
        await eventPublisher.PublishArticleUpdated(command.ArticleGuid, cancellationToken);
    }
}
