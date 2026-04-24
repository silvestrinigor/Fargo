using Fargo.Application.Authentication;
using Fargo.Application.Events;
using Fargo.Application.Partitions;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.Articles;
using Fargo.Domain.Partitions;

namespace Fargo.Application.Articles;

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
/// <item><description>Ensuring the target partition exists and is accessible, when provided.</description></item>
/// <item><description>Creating the article with an optional initial partition assignment.</description></item>
/// </list>
///
/// When no partition is specified, the article is created without any partition and is
/// publicly accessible to all authenticated actors.
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
    /// <exception cref="Users.UserNotAuthorizedFargoApplicationException">
    /// Thrown when the user does not have permission to create articles.
    /// </exception>
    /// <exception cref="PartitionNotFoundFargoApplicationException">
    /// Thrown when the specified partition does not exist.
    /// Only applicable when <c>firstPartition</c> is provided.
    /// </exception>
    /// <exception cref="PartitionAccessDeniedFargoApplicationException">
    /// Thrown when the current user does not have access to the specified partition.
    /// Only applicable when <c>firstPartition</c> is provided.
    /// </exception>
    /// <remarks>
    /// When <c>firstPartition</c> is provided, the article is created in that partition
    /// and the actor must have access to it. When <c>firstPartition</c> is
    /// <see langword="null"/>, the article is created without any partition and is
    /// publicly accessible to all authenticated actors.
    /// </remarks>
    public async Task<Guid> Handle(
            ArticleCreateCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.CreateArticle);

        Partition? partition = null;

        if (command.Article.FirstPartition.HasValue)
        {
            partition = await partitionRepository.GetFoundByGuid(command.Article.FirstPartition.Value, cancellationToken);

            actor.ValidateHasPartitionAccess(partition.Guid);
        }

        var article = new Article
        {
            Name = command.Article.Name,
            Description = command.Article.Description ?? Description.Empty,
            Mass = command.Article.Mass,
            LengthX = command.Article.LengthX,
            LengthY = command.Article.LengthY,
            LengthZ = command.Article.LengthZ
        };

        if (partition is not null)
        {
            article.Partitions.Add(partition);
        }

        articleRepository.Add(article);

        await unitOfWork.SaveChanges(cancellationToken);

        await eventPublisher.PublishArticleCreated(article.Guid, partition is null ? [] : [partition.Guid], cancellationToken);

        return article.Guid;
    }
}
