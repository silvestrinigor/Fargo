using Fargo.Application.Models.TreeModels;
using Fargo.Domain.ValueObjects;

namespace Fargo.HttpApi.Client.Contracts;

public interface ITreeClient
{
    Task<IReadOnlyCollection<TreeNode>> GetPartitionTreeAsync(
        Guid? parentPartitionGuid = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TreeNode>> GetUserGroupTreeAsync(
        Guid? userGroupGuid = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TreeNode>> GetPartitionSecurityTreeAsync(
        Guid? partitionGuid = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TreeNode>> GetArticleTreeAsync(
        Guid? articleGuid = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken cancellationToken = default);
}
