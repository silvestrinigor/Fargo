using Fargo.Sdk.Models;

namespace Fargo.Sdk.Managers;

public interface ITreeManager
{
    Task<IReadOnlyCollection<TreeNodeInfo>> GetPartitionTreeAsync(Guid? parentPartitionGuid = null, int? page = null, int? limit = null, CancellationToken ct = default);

    Task<IReadOnlyCollection<TreeNodeInfo>> GetUserGroupTreeAsync(Guid? userGroupGuid = null, int? page = null, int? limit = null, CancellationToken ct = default);

    Task<IReadOnlyCollection<TreeNodeInfo>> GetArticleTreeAsync(Guid? articleGuid = null, int? page = null, int? limit = null, CancellationToken ct = default);
}
