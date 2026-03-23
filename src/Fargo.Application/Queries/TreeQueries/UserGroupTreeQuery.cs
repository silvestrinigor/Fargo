using Fargo.Application.Extensions;
using Fargo.Application.Models.TreeModels;
using Fargo.Application.Repositories;
using Fargo.Application.Security;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.TreeQueries;

public sealed record UserGroupTreeQuery(
    Guid? UserGroupGuid = null,
    Pagination? Pagination = null)
    : IQuery<IReadOnlyCollection<TreeNode>>;

public sealed class UserGroupTreeQueryHandler(
        ActorService actorService,
    IUserGroupTreeRepository userGroupTreeRepository,
    ICurrentUser currentUser)
    : IQueryHandler<UserGroupTreeQuery, IReadOnlyCollection<TreeNode>>
{
    public async Task<IReadOnlyCollection<TreeNode>> Handle(
        UserGroupTreeQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedUserActorByGuid(currentUser.UserGuid, cancellationToken);

        return await userGroupTreeRepository.GetMembers(
            query.Pagination ?? Pagination.FirstPage20Items,
            actor.PartitionAccesses,
            query.UserGroupGuid,
            cancellationToken);
    }
}
