using Fargo.Application.Models.TreeModels;
using Fargo.Web.Api;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Fargo.Web.Features.Trees;

public sealed class UserGroupTreeApi(
    IHttpClientFactory httpClientFactory,
    FargoSession session,
    IOptions<JsonOptions> httpJsonOptions)
    : FargoApiClientBase(httpClientFactory, session, httpJsonOptions)
{
    public async Task<IReadOnlyCollection<EntityTreeNode>> GetChildrenAsync(
        Guid? userGroupGuid,
        CancellationToken cancellationToken = default)
    {
        var result = await GetFromJsonAsync<IReadOnlyCollection<EntityTreeNode>>(
            $"/trees/user-groups",
            cancellationToken: cancellationToken);

        return result ?? Array.Empty<EntityTreeNode>();
    }
}
