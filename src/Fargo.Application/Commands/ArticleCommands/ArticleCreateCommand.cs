using Fargo.Application.Events;
using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain;
using Fargo.Domain.Articles;
using Fargo.Domain.Partitions;

namespace Fargo.Application.Commands.ArticleCommands;

/// <summary>
/// Command used to create a new <see cref="Article"/>.
/// </summary>
/// <param name="Article">
/// The data required to create the article, including its name,
/// description, and optional target partition.
/// </param>
/// <remarks>
/// This command represents an intention to create an article while
/// respecting authorization and partition-based access rules.
/// </remarks>
public sealed record ArticleCreateCommand(
        ArticleCreateModel Article
        ) : ICommand<Guid>;

/// <summary>
/// Handles <see cref="ArticleCreateCommand"/>.
/// </summary>
/// <remarks>
/// This handler is responsible for:
/// <list type="bullet">
/// <item><description>Validating the current user's authorization.</description></item>
/// <item><description>Ensuring the target partition exists and is accessible.</description></item>
/// <item><description>Enforcing that every article belongs to at least one partition.</description></item>
/// </list>
///
/// The article is always associated with a valid partition to guarantee
/// proper data isolation and access control within the system.
/// </remarks>
public sealed class ArticleCreateCommandHandler(
        ActorService actorService,
        IArticleRepository articleRepository,
        IPartitionRepository partitionRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        IFargoEventPublisher eventPublisher
        ) : ICommandHandler<ArticleCreateCommand, Guid>
{
    /// <summary>
    /// Executes the command to create a new article.
    /// </summary>
    /// <param name="command">
    /// The command containing the data required for article creation.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The unique identifier of the newly created <see cref="Article"/>.
    /// </returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current user is not authenticated or inactive.
    /// </exception>
    /// <exception cref="UserNotAuthorizedFargoApplicationException">
    /// Thrown when the user does not have permission to create articles.
    /// </exception>
    /// <exception cref="PartitionNotFoundFargoApplicationException">
    /// Thrown when the specified partition does not exist.
    /// </exception>
    /// <exception cref="PartitionAccessDeniedFargoApplicationException">
    /// Thrown when the current user does not have access to the specified partition.
    /// </exception>
    /// <remarks>
    /// The article is created in the specified partition. If no partition is
    /// explicitly provided, the global partition is used as a fallback.
    ///
    /// This ensures that every article is always associated with at least one
    /// partition, enforcing partition-based isolation and access control.
    /// </remarks>
    public async Task<Guid> Handle(
            ArticleCreateCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.CreateArticle);

        var partitionGuid = command.Article.FirstPartition ?? PartitionService.GlobalPartitionGuid;

        var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

        actor.ValidateHasPartitionAccess(partition.Guid);

        var article = new Article
        {
            Name = command.Article.Name,
            Description = command.Article.Description ?? Description.Empty,
            Mass = command.Article.Mass,
            LengthX = command.Article.LengthX,
            LengthY = command.Article.LengthY,
            LengthZ = command.Article.LengthZ
        };

        article.Partitions.Add(partition);

        articleRepository.Add(article);

        await unitOfWork.SaveChanges(cancellationToken);

        await eventPublisher.PublishArticleCreated(article.Guid, [partition.Guid], cancellationToken);

        return article.Guid;
    }
}
