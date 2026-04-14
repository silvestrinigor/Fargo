using Fargo.Application.Events;
using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Models.BarcodeModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Commands.ArticleCommands;

/// <summary>
/// Command used to add a barcode to an article.
/// </summary>
/// <param name="ArticleGuid">The unique identifier of the article.</param>
/// <param name="Barcode">The barcode data to add.</param>
public sealed record ArticleAddBarcodeCommand(
    Guid ArticleGuid,
    BarcodeAddModel Barcode
    ) : ICommand<Guid>;

/// <summary>
/// Handles <see cref="ArticleAddBarcodeCommand"/>.
/// </summary>
public sealed class ArticleAddBarcodeCommandHandler(
    ActorService actorService,
    IArticleRepository articleRepository,
    IBarcodeRepository barcodeRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IFargoEventPublisher eventPublisher
    ) : ICommandHandler<ArticleAddBarcodeCommand, Guid>
{
    /// <summary>
    /// Executes the command to add a barcode to an article.
    /// </summary>
    /// <param name="command">The command containing article and barcode data.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The unique identifier of the newly created barcode.</returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current actor is not authenticated or inactive.
    /// </exception>
    /// <exception cref="UserNotAuthorizedFargoApplicationException">
    /// Thrown when the actor does not have <see cref="ActionType.AddBarcode"/> permission.
    /// </exception>
    /// <exception cref="ArticleNotFoundFargoApplicationException">
    /// Thrown when the article does not exist or is not accessible.
    /// </exception>
    /// <exception cref="BarcodeAlreadyExistsFargoApplicationException">
    /// Thrown when the article already has a barcode with the same format.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the barcode code is invalid for the specified format.
    /// </exception>
    public async Task<Guid> Handle(
        ArticleAddBarcodeCommand command,
        CancellationToken cancellationToken = default
        )
    {
        ArgumentNullException.ThrowIfNull(command);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.AddBarcode);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        if (article.Barcodes.Any(b => b.Format == command.Barcode.Format))
        {
            throw new BarcodeAlreadyExistsFargoApplicationException(command.ArticleGuid, command.Barcode.Format);
        }

        var value = new BarcodeValue(command.Barcode.Code, command.Barcode.Format);

        var barcode = new Barcode
        {
            Value = value,
            Article = article
        };

        barcodeRepository.Add(barcode);

        await unitOfWork.SaveChanges(cancellationToken);

        await eventPublisher.PublishArticleUpdated(article.Guid, cancellationToken);

        return barcode.Guid;
    }
}
