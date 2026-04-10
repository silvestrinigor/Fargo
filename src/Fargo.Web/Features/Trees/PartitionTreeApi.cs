using Fargo.Application.Models.TreeModels;
using Fargo.Web.Api;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Fargo.Web.Features.Trees;

public sealed class PartitionTreeApi(
    IHttpClientFactory httpClientFactory,
    FargoSession session,
    IOptions<JsonOptions> httpJsonOptions)
    : FargoApiClientBase(httpClientFactory, session, httpJsonOptions)
{
    public async Task<IReadOnlyCollection<EntityTreeNode>> GetChildrenAsync(
        Guid? parentPartitionGuid,
        CancellationToken cancellationToken = default)
    {
        var result = await GetFromJsonAsync<IReadOnlyCollection<EntityTreeNode>>(
            $"/trees/partitions",
            cancellationToken: cancellationToken);

        return result ?? Array.Empty<EntityTreeNode>();
    }
}
