using Fargo.Application.Extensions;
using Fargo.Application.Models.TreeModels;
using Fargo.Application.Repositories;
using Fargo.Application.Security;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.TreeQueries;

public sealed record UserTreeQuery(
    Guid? UserGroupGuid = null,
    Pagination? Pagination = null)
    : IQuery<IReadOnlyCollection<EntityTreeNode>>;

public sealed class UserTreeQueryHandler(
    ActorService actorService,
    IUserTreeRepository userGroupTreeRepository,
    ICurrentUser currentUser)
    : IQueryHandler<UserTreeQuery, IReadOnlyCollection<EntityTreeNode>>
{
    public async Task<IReadOnlyCollection<EntityTreeNode>> Handle(
        UserTreeQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        if (actor.IsAdmin || actor.IsSystem)
        {
            return await userGroupTreeRepository.GetUserTreeNodes(
                query.Pagination ?? Pagination.FirstPage20Items,
                query.UserGroupGuid,
                cancellationToken);
        }

        return await userGroupTreeRepository.GetUserTreeNodesInPartitions(
            query.Pagination ?? Pagination.FirstPage20Items,
            actor.PartitionAccesses,
            query.UserGroupGuid,
            cancellationToken);
    }
}
