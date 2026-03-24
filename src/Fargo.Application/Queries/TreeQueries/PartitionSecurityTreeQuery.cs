using Fargo.Application.Extensions;
using Fargo.Application.Models.TreeModels;
using Fargo.Application.Repositories;
using Fargo.Application.Security;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.TreeQueries;

public sealed record PartitionSecurityTreeQuery(
    Guid? PartitionGuid = null,
    Pagination? Pagination = null)
    : IQuery<IReadOnlyCollection<TreeNode>>;

public sealed class PartitionSecurityTreeQueryHandler(
        ActorService actorService,
    IPartitionSecurityTreeRepository partitionSecurityTreeRepository,
    ICurrentUser currentUser)
    : IQueryHandler<PartitionSecurityTreeQuery, IReadOnlyCollection<TreeNode>>
{
    public async Task<IReadOnlyCollection<TreeNode>> Handle(
        PartitionSecurityTreeQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        return await partitionSecurityTreeRepository.GetMembers(
            query.Pagination ?? Pagination.FirstPage20Items,
            actor.PartitionAccesses,
            query.PartitionGuid,
            cancellationToken);
    }
}
