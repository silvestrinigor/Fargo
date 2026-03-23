using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Commands.ArticleCommands;

/// <summary>
/// Command used to create a new <see cref="Article"/>.
/// </summary>
/// <param name="Article">
/// The data required to create the article, including the partition
/// in which the article will be created.
/// </param>
public sealed record ArticleCreateCommand(
    ArticleCreateModel Article
    ) : ICommand<Guid>;

/// <summary>
/// Handles the execution of <see cref="ArticleCreateCommand"/>.
/// </summary>
/// <remarks>
/// This handler creates a new <see cref="Article"/> in a partition
/// accessible to the current user. Every article must belong to
/// at least one partition.
/// </remarks>
public sealed class ArticleCreateCommandHandler(
    ActorService actorService,
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<ArticleCreateCommand, Guid>
{
    /// <summary>
    /// Executes the command to create a new article.
    /// </summary>
    /// <param name="command">
    /// The command containing article creation data.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The unique identifier of the created article.
    /// </returns>
    /// <exception cref="PartitionNotFoundFargoApplicationException">
    /// Thrown when the specified partition does not exist.
    /// </exception>
    /// <exception cref="PartitionAccessDeniedFargoApplicationException">
    /// Thrown when the current user does not have access to the specified partition.
    /// </exception>
    /// <remarks>
    /// The article is created in the specified partition. If no partition is
    /// explicitly provided, the global partition is used.
    /// </remarks>
    public async Task<Guid> Handle(
        ArticleCreateCommand command,
        CancellationToken cancellationToken = default
        )
    {
        var actor = await actorService.GetAuthorizedUserActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHassPermission(ActionType.CreateArticle);

        var partitionGuid = command.Article.FirstPartition ?? PartitionService.GlobalPartitionGuid;

        var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

        actor.ValidateHassPartitionAccess(partition.Guid);

        var article = new Article
        {
            Name = command.Article.Name,
            Description = command.Article.Description ?? Description.Empty
        };

        article.Partitions.Add(partition);

        articleRepository.Add(article);

        await unitOfWork.SaveChanges(cancellationToken);

        return article.Guid;
    }
}
