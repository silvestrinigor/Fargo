using Fargo.Sdk.Http;
using Fargo.Sdk.Models;

namespace Fargo.Sdk.Trees;

internal sealed class TreeClient : ITreeClient
{
    private readonly FargoHttpClient httpClient;

    public TreeClient(FargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<TreeNodeInfo>> GetPartitionTreeAsync(Guid? parentPartitionGuid = null, int? page = null, int? limit = null, CancellationToken ct = default)
    {
        var query = FargoHttpClient.BuildQuery(
            ("parentPartitionGuid", parentPartitionGuid?.ToString()),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()));

        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<TreeNodeInfo>>($"/trees/partitions{query}", ct) ?? [];
    }

    public async Task<IReadOnlyCollection<TreeNodeInfo>> GetUserGroupTreeAsync(Guid? userGroupGuid = null, int? page = null, int? limit = null, CancellationToken ct = default)
    {
        var query = FargoHttpClient.BuildQuery(
            ("userGroupGuid", userGroupGuid?.ToString()),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()));

        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<TreeNodeInfo>>($"/trees/user-groups{query}", ct) ?? [];
    }

    public async Task<IReadOnlyCollection<TreeNodeInfo>> GetArticleTreeAsync(Guid? articleGuid = null, int? page = null, int? limit = null, CancellationToken ct = default)
    {
        var query = FargoHttpClient.BuildQuery(
            ("articleGuid", articleGuid?.ToString()),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()));

        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<TreeNodeInfo>>($"/trees/articles{query}", ct) ?? [];
    }
}
