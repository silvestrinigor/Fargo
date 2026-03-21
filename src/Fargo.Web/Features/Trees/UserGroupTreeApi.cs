using Fargo.Application.Models.TreeModels;
using Fargo.Web.Api;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Fargo.Web.Features.Trees;

public sealed class UserGroupTreeApi(
    IHttpClientFactory httpClientFactory,
    ClientSessionAccessor sessionAccessor,
    IOptions<JsonOptions> httpJsonOptions)
    : FargoApiClientBase(httpClientFactory, sessionAccessor, httpJsonOptions)
{
    public async Task<IReadOnlyCollection<TreeNode>> GetChildrenAsync(
        Guid? userGroupGuid,
        CancellationToken cancellationToken = default)
    {
        var result = await GetFromJsonAsync<IReadOnlyCollection<TreeNode>>(
            $"/trees/user-groups?userGroupGuid={userGroupGuid}",
            cancellationToken: cancellationToken);

        return result ?? Array.Empty<TreeNode>();
    }
}
