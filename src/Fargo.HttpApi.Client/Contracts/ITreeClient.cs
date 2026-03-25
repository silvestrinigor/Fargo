using Fargo.Application.Models.TreeModels;
using Fargo.Domain.ValueObjects;

namespace Fargo.HttpApi.Client.Contracts;

public interface ITreeClient
{
    Task<IReadOnlyCollection<EntityTreeNode>> GetPartitionTreeAsync(
        Guid? parentPartitionGuid = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<EntityTreeNode>> GetUserGroupTreeAsync(
        Guid? userGroupGuid = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<EntityTreeNode>> GetPartitionSecurityTreeAsync(
        Guid? partitionGuid = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<EntityTreeNode>> GetArticleTreeAsync(
        Guid? articleGuid = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken cancellationToken = default);
}
