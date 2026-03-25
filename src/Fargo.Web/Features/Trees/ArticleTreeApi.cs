using Fargo.Application.Models.TreeModels;
using Fargo.Web.Api;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Fargo.Web.Features.Trees;

public sealed class ArticleTreeApi(
    IHttpClientFactory httpClientFactory,
    ClientSessionAccessor sessionAccessor,
    IOptions<JsonOptions> httpJsonOptions)
    : FargoApiClientBase(httpClientFactory, sessionAccessor, httpJsonOptions)
{
    public async Task<IReadOnlyCollection<EntityTreeNode>> GetChildrenAsync(
        Guid? articleGuid,
        CancellationToken cancellationToken = default)
    {
        var result = await GetFromJsonAsync<IReadOnlyCollection<EntityTreeNode>>(
            $"/trees/articles",
            cancellationToken: cancellationToken);

        return result ?? Array.Empty<EntityTreeNode>();
    }
}
