using Fargo.Application.Models.TreeModels;
using Fargo.Domain.ValueObjects;
using Fargo.HttpApi.Client.Contracts;
using Fargo.Infrastructure.Client.Http;

namespace Fargo.Infrastructure.Client.Clients;

public sealed class TreeClient(HttpClient http)
    : FargoHttpClientBase(http), ITreeClient
{
    public Task<IReadOnlyCollection<EntityTreeNode>> GetPartitionTreeAsync(
        Guid? parentPartitionGuid = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken cancellationToken = default)
    {
        var uri =
            $"/trees/partitions?parentPartitionGuid={parentPartitionGuid}&page={page}&limit={limit}";

        return GetCollectionAsync<EntityTreeNode>(uri, cancellationToken);
    }

    public Task<IReadOnlyCollection<EntityTreeNode>> GetUserGroupTreeAsync(
        Guid? userGroupGuid = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken cancellationToken = default)
    {
        var uri =
            $"/trees/user-groups?userGroupGuid={userGroupGuid}&page={page}&limit={limit}";

        return GetCollectionAsync<EntityTreeNode>(uri, cancellationToken);
    }

    public Task<IReadOnlyCollection<EntityTreeNode>> GetPartitionSecurityTreeAsync(
        Guid? partitionGuid = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken cancellationToken = default)
    {
        var uri =
            $"/trees/partitions/security?partitionGuid={partitionGuid}&page={page}&limit={limit}";

        return GetCollectionAsync<EntityTreeNode>(uri, cancellationToken);
    }

    public Task<IReadOnlyCollection<EntityTreeNode>> GetArticleTreeAsync(
        Guid? articleGuid = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken cancellationToken = default)
    {
        var uri =
            $"/trees/articles?articleGuid={articleGuid}&page={page}&limit={limit}";

        return GetCollectionAsync<EntityTreeNode>(uri, cancellationToken);
    }
}
