using Fargo.Application.Extensions;
using Fargo.Application.Models.TreeModels;
using Fargo.Application.Repositories;
using Fargo.Application.Security;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.TreeQueries;

public sealed record PartitionTreeQuery(
    Guid? ParentPartitionGuid = null,
    Pagination? Pagination = null)
    : IQuery<IReadOnlyCollection<TreeNode>>;

public sealed class PartitionTreeQueryHandler(
        ActorService actorService,
    IPartitionTreeRepository partitionTreeRepository,
    ICurrentUser currentUser)
    : IQueryHandler<PartitionTreeQuery, IReadOnlyCollection<TreeNode>>
{
    public async Task<IReadOnlyCollection<TreeNode>> Handle(
        PartitionTreeQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedUserActorByGuid(currentUser.UserGuid, cancellationToken);

        return await partitionTreeRepository.GetMembers(
            query.Pagination ?? Pagination.FirstPage20Items,
            actor.PartitionAccesses,
            query.ParentPartitionGuid,
            cancellationToken);
    }
}
