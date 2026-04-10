using Fargo.Application.Models.TreeModels;
using Fargo.Web.Api;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Fargo.Web.Features.Trees;

public sealed class PartitionSecurityTreeApi(
    IHttpClientFactory httpClientFactory,
    FargoSession session,
    IOptions<JsonOptions> httpJsonOptions)
    : FargoApiClientBase(httpClientFactory, session, httpJsonOptions)
{
    public async Task<IReadOnlyCollection<EntityTreeNode>> GetChildrenAsync(
        Guid? partitionGuid,
        CancellationToken cancellationToken = default)
    {
        var result = await GetFromJsonAsync<IReadOnlyCollection<EntityTreeNode>>(
            $"/trees/partitions/security",
            cancellationToken: cancellationToken);

        return result ?? Array.Empty<EntityTreeNode>();
    }
}
